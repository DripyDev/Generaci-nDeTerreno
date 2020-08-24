using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class GeneradorMapa : MonoBehaviour {
    ///<summary>Tipos de dibujo posibles, uno para dibujar a color y otro para blanco y negro</summary>
    public enum DrawMode {NoiseMap, ColourMap, Mesh, FalloffMap};
    public DrawMode modoDibujo;
    public Ruido.NormalizeMode normalizeMode;
    [Range(0,6)]//Toma los valores 2,4,6,8,10 y 12 asi que es el valor*2 (0 es caso especial que habra que controlar)
    public int nivelDetalleEditor;

    //241 porque unity solo permite 65025 (255^2) vertices por mesh, asi que 241 es un buen numero dentro del rango. Ahora sustituye a alctura y anchura porque siempre sera un mapa cuadrado
    public const int tamañoMapaChunk = 241;

    //Cuanto 'zoom' hacemos sobre el ruido
    public float escaladoRuido;

    //Parametros extra para el ruido

    public int octaves;
    public float lacunaridad;
    [Range(0,1)]
    public float persistencia;
    public int seed;
    public Vector2 offset;
    //Multiplicador de alturas para que la altura de cada punto varie mucho mas porque sino siempre son valores entre 0 y 1
    public float multiplicadorAlturaMesh;
    //Curva que vamos a usar para suavizar valores dentro de los rangos que queramos
    public AnimationCurve curbaAlturaMesh;
    //Indica si queremos usar o no el mapa Falloff o no
    public bool useFalloff;
    float[,] falloffMap;

    ///<summary>Tipos de terrenos diferentes para dibujar el mapa</summary>
    public TipoTerreno[] terrenos;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public bool autoUpdate;

    void Awake(){
        falloffMap = FalloffGenerator.GenerateFalloffMap(tamañoMapaChunk);
    }

    public void DrawMapInEditor(){
        MapData mapD = GenerarMapData(Vector2.zero); 
        //Obtenemos el script MapDisplay que esta en nuestro mismo GameObject
        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
        if(modoDibujo == DrawMode.NoiseMap)
            mapDisplay.DibujarTextura(GeneradorTextura.TextureFromHeigthMap(mapD.mapaAltura));
        else if(modoDibujo == DrawMode.ColourMap)
            mapDisplay.DibujarTextura(GeneradorTextura.TextureFromColourMap(mapD.mapaColores, tamañoMapaChunk, tamañoMapaChunk));
        else if(modoDibujo == DrawMode.Mesh)
            mapDisplay.DibujarMesh(MeshGenerator.GenerateTerrainMesh(mapD.mapaAltura, multiplicadorAlturaMesh, curbaAlturaMesh, nivelDetalleEditor), GeneradorTextura.TextureFromColourMap(mapD.mapaColores, tamañoMapaChunk, tamañoMapaChunk));
        else if(modoDibujo == DrawMode.FalloffMap)
            mapDisplay.DibujarTextura(GeneradorTextura.TextureFromHeigthMap(FalloffGenerator.GenerateFalloffMap(tamañoMapaChunk)));
    }

    public void RequesMapData(Vector2 centro, Action<MapData> callback){
        ThreadStart threadStart = delegate{
            MapDataThread(centro, callback);
        };
        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centro, Action<MapData> callback){
        MapData mapData = GenerarMapData(centro);
        //Nos aseguramos que ningun otro thread entre en esta informacion, se bloquea para el primero que lo coja
        lock (mapDataThreadInfoQueue){
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapdata, int lod, Action<MeshData> callback){
        ThreadStart threadStart = delegate {
            MeshDataThread(mapdata, lod, callback);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapdata, int lod, Action<MeshData> callback){
        MeshData meshdata = MeshGenerator.GenerateTerrainMesh(mapdata.mapaAltura, multiplicadorAlturaMesh, curbaAlturaMesh, lod);
        lock(meshDataThreadInfoQueue) {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshdata));
        }
    }

    void Update() {
        if(mapDataThreadInfoQueue.Count > 0){
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++) {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
        if(meshDataThreadInfoQueue.Count>0){
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++){
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    private MapData GenerarMapData(Vector2 centro){
        //Mapa de ruido. El penultimo parametro es para que genere mapas diferentes en diferentes chunks
        float[,] mapaRuido = Ruido.GeneradorRuido(tamañoMapaChunk, tamañoMapaChunk, seed, escaladoRuido, octaves, persistencia, lacunaridad, centro + offset, normalizeMode);

        //Mapa de colores
        Color[] mapaColores = new Color[tamañoMapaChunk * tamañoMapaChunk];
        for (int y = 0; y < tamañoMapaChunk; y++) {
            for (int x = 0; x < tamañoMapaChunk; x++) {
                if(useFalloff){
                    mapaRuido[x,y] = Mathf.Clamp01(mapaRuido[x,y] - falloffMap[x,y]);
                }

                float alturaActual = mapaRuido[x,y];
                for (int i = 0; i < terrenos.Length; i++) {
                    //Devolvemos el color dentro del cual este la altura de este punto
                    if(alturaActual >= terrenos[i].altura){
                        mapaColores[y * tamañoMapaChunk + x] = terrenos[i].color;
                    }
                    else{
                        break;
                    }
                }
            }
        }
        return new MapData(mapaRuido, mapaColores);  

    }

    //Se llama cada vez que se altera un valor del editor. Aprovechamos para comprobar que haya valores validos
    void OnValidate() {
        if(octaves < 0)
            octaves = 0;
        if(lacunaridad < 1)
            lacunaridad = 1;
        falloffMap = FalloffGenerator.GenerateFalloffMap(tamañoMapaChunk);
    }

    //Para que se pueda crear desde el editor
    [System.Serializable]
    public struct TipoTerreno{
        public string nombre;
        public float altura;
        public Color color;
    }
}
//Estan fuera de la calse para que sean accesibles desde otros archivos/clases
public struct MapData{
    public readonly float[,] mapaAltura;
    public readonly Color[] mapaColores;
    public MapData(float[,] mapaAltura, Color[] mapaColores){
        this.mapaAltura = mapaAltura;
        this.mapaColores = mapaColores;
    }
}

struct MapThreadInfo<T>{
    public readonly Action<T> callback;
    public readonly T parameter;
    public MapThreadInfo(Action<T> callback, T param){
        this.callback = callback;
        this.parameter = param;
    }
}


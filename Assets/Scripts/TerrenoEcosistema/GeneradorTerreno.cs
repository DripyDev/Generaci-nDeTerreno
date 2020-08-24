using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneradorTerreno : MonoBehaviour {
    public enum ModoDibujo{MapaRuido, MapaColores, Mesh, MapaFalloff};
    public ModoDibujo modoDibujo;
    public enum ModoAltura{Enteros, Floats};
    public ModoAltura modoAltura;

    ///<summary>Dimensiones del mundo que siempre sera cuadrado</summary>
    public int dimensionMapa;
    ///<summary>Numero de niveles de detalle que tiene el ruid (como el numero de capas que componen el ruido)</summary>
    public int octaves;
    ///<summary>Determina cuanto contribuye cada octave a la forma general. (ajusta la amplitud)</summary>
    [Range(0,1)]
    public float persistencia;
    ///<summary>Determina cuanto detalles es añadido o eliminado en cada octave. Ajusta la frecuencia</summary>
    public float lacunaridad;
    ///<summary>'Zoom' sobre el ruido perlin</summary>
    public int escala;
    ///<summary>Cuanto desplazamos el ruido</summary>
    public Vector2 offset;
    public int multiplicadorAltura;
    //No hace falta porque al no tener chunks siempre sera local
    //public Ruido.NormalizeMode modoNormalizar;
    public int semilla;
    public bool autoUpdate;

    [Header("Tipos de terreno")]
    public TipoTerreno[] tiposTerrenos;
    public bool usarFalloff;
    //Informacion del mapa para que use el ecosistema
    public DatosTerreno datosTerreno;
    public AnimationCurve curva;

    private float[,] mapaRuido;
    private float[,] falloffMap;
    private Color[] mapaColores;
    private Mesh mesh;

    void Awake() {
        GenerarMeshTerreno();
    }

    public Mesh GenerarMeshTerreno(){
        //Mapa de ruido
        mapaRuido = Ruido.GeneradorRuido(dimensionMapa, dimensionMapa, semilla, escala, octaves, persistencia, lacunaridad, offset, Ruido.NormalizeMode.Local);
        falloffMap = FalloffGenerator.GenerateFalloffMap(dimensionMapa);

        //Inicializamos con floats porque si, para que este inicializado con algo
        DatosMesh datosMesh = DatosMeshFloats();
        if(modoAltura == ModoAltura.Floats){
            datosMesh = DatosMeshFloats();
        }
        else if(modoAltura == ModoAltura.Enteros){
            datosMesh = DatosMeshEnteros();
        }

        mesh = CrearMesh(datosMesh.vertices, datosMesh.triangulos, datosMesh.uvs);
        return mesh;
    }

    ///<summary>Devuelve una estructura con los vertices, uvs, triangulos calculados para el mesh con numeros flotantes (en el futuro tambien normales)</summary>
    private DatosMesh DatosMeshFloats(){
        Vector3[] vertices = new Vector3[dimensionMapa * dimensionMapa];
        int[] triangulos = new int[(dimensionMapa-1) * (dimensionMapa-1) * 6];
        Vector2[] uvs = new Vector2[dimensionMapa * dimensionMapa];

        float topLeftX = (dimensionMapa -1) / -2f;
        float topLeftZ = (dimensionMapa -1) / 2f;

        int indiceVertice = 0;
        int indiceTriangulo = 0;
        for (int y = 0; y < dimensionMapa; y++) {    
            for (int x = 0; x < dimensionMapa; x++) {
                //Alturas no enteras
                vertices[indiceVertice] = new Vector3(topLeftX + x, curva.Evaluate(mapaRuido[x,y]) * multiplicadorAltura, topLeftZ - y);
                uvs[indiceVertice] = new Vector2(x/(float) dimensionMapa, y/(float)dimensionMapa);

                if(x < dimensionMapa -1 && y < dimensionMapa -1){
                    //Añadimos los dos triangulos
                    triangulos[indiceTriangulo] = indiceVertice;
                    triangulos[indiceTriangulo+1] = indiceVertice + (dimensionMapa) + 1;
                    triangulos[indiceTriangulo+2] = indiceVertice + (dimensionMapa);

                    triangulos[indiceTriangulo+3] = indiceVertice;
                    triangulos[indiceTriangulo+4] = indiceVertice + 1;
                    triangulos[indiceTriangulo+5] = indiceVertice + (dimensionMapa) + 1;
                    indiceTriangulo += 6;
                }
                
                indiceVertice++;
            }
        }
        return new DatosMesh(vertices, triangulos, uvs);
    }

    public bool prueba;
    ///<summary>Devuelve una estructura con los vertices, uvs, triangulos calculados para el mesh formado por casillas (en el futuro tambien normales)</summary>
    private DatosMesh DatosMeshEnteros(){
        //Multiplicamos por 4 porque va a haber mas vertices
        /*Vector3[] vertices = new Vector3[4 * dimensionMapa * dimensionMapa];
        int[] triangulos = new int[4 * (dimensionMapa-1) * (dimensionMapa-1) * 6];
        Vector2[] uvs = new Vector2[4 * dimensionMapa * dimensionMapa];
        */
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangulos = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        Vector3[,] centros = new Vector3[dimensionMapa, dimensionMapa];
        bool[,] caminables = new bool[dimensionMapa, dimensionMapa];
        bool[,] shore = new bool[dimensionMapa, dimensionMapa];
        int[,] indicesTerreno = new int[dimensionMapa, dimensionMapa];

        //Origen de donde partimos en el mapa
        float topLeftX = (dimensionMapa -1) / -2f;
        float topLeftZ = (dimensionMapa -1) / 2f;

        int indiceVertice = 0;
        //Vectores Arriba-Abajo-Izquierda-Derecha
        Vector3[] aaid = {Vector3.up, Vector3.down, Vector3.left, Vector3.right};
        for (int y = 0; y < dimensionMapa; y++) {    
            for (int x = 0; x < dimensionMapa; x++) {
                indiceVertice = vertices.Count;
                int[][] verticesLateralesPorDireccion = { new int[] { 0, 1 }, new int[] { 3, 2 }, new int[] { 2, 0 }, new int[] { 1, 3 } };

                //Primer prototipo de altura de enteros 
                //vertices[indiceVertice] = new Vector3(topLeftX + x, ProcesarAlturaEnteros(mapaRuido[x,y]), topLeftZ - y);
                //Vamos a introducir una casilla entera para la posicion (x,y) del mundo
                //float min = -dimensionMapa / 2f;
                int altura = ProcesarAlturaEnteros(mapaRuido[x,y]);
                //Puntos de la casilla. NorOeste, NoerEste, SurEste, SurOeste
                Vector3 NO = new Vector3( topLeftX + x, altura, topLeftZ - y);
                Vector3 NE = NO + Vector3.right;
                Vector3 SO = NO - Vector3.forward;
                Vector3 SE = SO + Vector3.right;

                Vector3[] casilla = {NO, NE, SO, SE};
                vertices.AddRange(casilla);

                Vector2 uv = new Vector2(x/(float) dimensionMapa, y/(float)dimensionMapa);
                Vector2[] uvsCasilla = {uv, uv, uv, uv};
                uvs.AddRange(uvsCasilla);

                if(x < dimensionMapa  && y < dimensionMapa ){
                    //Añadimos los dos triangulos
                    triangulos.Add(indiceVertice);
                    triangulos.Add(indiceVertice + 3);
                    triangulos.Add(indiceVertice + 2);

                    triangulos.Add(indiceVertice);
                    triangulos.Add(indiceVertice + 1);
                    triangulos.Add(indiceVertice + 3);
                }

                bool esEsquina = x == 0|| x == dimensionMapa || y == 0|| y == dimensionMapa;
                bool esAgua = altura == 0;
                if(prueba){
                    if(!esAgua || esEsquina){
                        //Recorremos los vecinos para rellenar huecos entre diferentes alturas y las esquinas del mapa
                        for (int i = 0; i < aaid.Length; i++){
                            int vecinosX = x + (int)aaid[i].x;
                            int vecinosY = y + (int)aaid[i].y;
                            
                            bool vecinoDiferenteAltura = false;
                            bool vecinoAgua=false;
                            bool vecinoFueraRango = vecinosX < 0 || vecinosX >= dimensionMapa || vecinosY < 0 || vecinosY >= dimensionMapa;
                            int alturaVecino = 0;
                            //Solo sacamos la altura del vecino si no estamos fuera de rango
                            if(!vecinoFueraRango){
                                alturaVecino = ProcesarAlturaEnteros(mapaRuido[vecinosX, vecinosY]);
                                vecinoAgua = alturaVecino == 0;
                                vecinoDiferenteAltura = alturaVecino != altura;
                                if(vecinoAgua){
                                    shore[vecinosX, vecinosY] = true;
                                }
                            }

                            //Necesitamos una casilla de altura o de esquinas
                            if(vecinoFueraRango || vecinoDiferenteAltura){
                                //Profundidad de la casilla que tenemos que poner para tapar agujeros.
                                //Si el vecino esta fuera de rango estamos en una esquina, sino es para poner el agua
                                float profundidad = vecinoFueraRango? altura+1:Mathf.Abs(alturaVecino - altura);

                                indiceVertice = vertices.Count;
                                int indiceVerticeCasilla1 = verticesLateralesPorDireccion[i][0];
                                int indiceVerticeCasilla2 = verticesLateralesPorDireccion[i][1];
                                vertices.Add (casilla[indiceVerticeCasilla1]);
                                vertices.Add (casilla[indiceVerticeCasilla1] + Vector3.down * profundidad);
                                vertices.Add (casilla[indiceVerticeCasilla2]);
                                vertices.Add (casilla[indiceVerticeCasilla2] + Vector3.down * profundidad);

                                uvs.AddRange (new Vector2[] { uv, uv, uv, uv });
                                int[] indicesTriangulos = { indiceVertice, indiceVertice + 3, indiceVertice + 2, indiceVertice, indiceVertice + 1, indiceVertice + 3};
                                triangulos.AddRange (indicesTriangulos);
                                //normals.AddRange (new Vector3[] { sideNormalsByDir[i], sideNormalsByDir[i], sideNormalsByDir[i], sideNormalsByDir[i] });
                            }
                        }
                    }
                }
                //Datos del terreno para el ecosistema
                centros[x,y] = NO + new Vector3(0.5f, 0, -0.5f);
                caminables[x,y] = altura > 0;
                indicesTerreno[x,y] = altura;
            }
        }
        datosTerreno = new DatosTerreno(centros, caminables, shore, indicesTerreno);
        /*if(vertices.Count >= 65025){
            print("CUIDADO. SE HA SUPERADO EL MAXIMO DE VERTICES EN UN MESH EN UNITY QUE ES 65025");
            print("Numero vertices: " + vertices.Count);
        }*/
        return new DatosMesh(vertices.ToArray(), triangulos.ToArray(), uvs.ToArray());
    }

    private void GenerarMapaColores() {
        //Mapa de colores
        mapaColores = new Color[dimensionMapa * dimensionMapa];
        for (int y = 0; y < dimensionMapa; y++) {
            for (int x = 0; x < dimensionMapa; x++) {
                if(usarFalloff){
                    mapaRuido[x,y] = Mathf.Clamp01(mapaRuido[x,y] - falloffMap[x,y]);
                }

                //NOTA:REPASAR ESTO QUE DA FALLOS POR EL >= CREO
                float alturaActual = mapaRuido[x,y];
                for (int i = 0; i < tiposTerrenos.Length; i++) {
                    //Devolvemos el color dentro del cual este la altura de este punto
                    //if(alturaActual >= tiposTerrenos[i].altura){
                    if(alturaActual < tiposTerrenos[i].altura){
                        mapaColores[y * dimensionMapa + x] = tiposTerrenos[i].color;
                        break;
                    }
                    //NOTA: QUITAR ESTO PARA EXPLICARLO EN LA DOCUMENTACION Y PODER ENSEÑAR EL EJEMPLO
                    //Para evitar aquellos valores superiores a 1 que a veces salen del ruido perlin
                    if(i==tiposTerrenos.Length-1){
                        mapaColores[y * dimensionMapa + x] = tiposTerrenos[i].color;
                    }
                    //else{
                    //    break;
                    //}
                }
            }
        }
    }

    public Mesh CrearMesh(Vector3[] vertices, int[] triangulos, Vector2[] uvs){
        Mesh mesh2 = new Mesh();
        //Para permitir un mayor numero de vertices
        mesh2.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh2.SetVertices(vertices);
        mesh2.SetTriangles(triangulos, 0, true);
        mesh2.SetUVs(0, uvs);
        mesh2.RecalculateNormals();
        return mesh2;
    }

    public int ProcesarAlturaEnteros(float altura){
        for (int i = 0; i < tiposTerrenos.Length; i++) {
            if(altura <= tiposTerrenos[i].altura){
                return i;
            }
        }
        print("Una altura diferente a las de los terrenos");
        return 0;
    }

    //Se llama cada vez que se cambia algo en el inspector
    void OnValidate() {
        if(octaves < 0)
            octaves = 0;
        if(lacunaridad < 1)
            lacunaridad = 1;
        falloffMap = FalloffGenerator.GenerateFalloffMap(dimensionMapa);
    }

    public void DrawMapInEditor(){
        GenerarMeshTerreno();
        GenerarMapaColores();
        //Obtenemos el script MapDisplay que esta en nuestro mismo GameObject
        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
        if(modoDibujo == ModoDibujo.MapaRuido)
            mapDisplay.DibujarTextura(TextureGenerator.TextureFromHeigthMap(mapaRuido));
        else if(modoDibujo == ModoDibujo.MapaColores)
            mapDisplay.DibujarTextura(GeneradorTextura.TextureFromColourMap(mapaColores, dimensionMapa, dimensionMapa));
        else if(modoDibujo == ModoDibujo.Mesh){
            //mapDisplay.DibujarMesh2(mesh, TextureGenerator.TextureFromHeigthMap(mapaRuido));
            mapDisplay.DibujarMesh2(mesh, TextureGenerator.TextureFromColourMap(mapaColores, dimensionMapa, dimensionMapa));
        }
        else if(modoDibujo == ModoDibujo.MapaFalloff)
            mapDisplay.DibujarTextura(GeneradorTextura.TextureFromHeigthMap(FalloffGenerator.GenerateFalloffMap(dimensionMapa)));
    }

    //Para que se pueda crear desde el editor
    [System.Serializable]
    public struct TipoTerreno{
        public string nombre;
        public float altura;
        public Color color;
    }

    public struct DatosMesh{
        public Vector3[] vertices;
        public int[] triangulos;
        public Vector2[] uvs;
        //NOTA: EN EL FUTURO CALCULAR LAS NORMALES
        //public Vector3[] normals;
        public DatosMesh(Vector3[] v, int[] t, Vector2[] u){
            this.vertices = v;
            this.triangulos = t;
            this.uvs = u;
            //this.normals = n;
        }
    }

    //Datos necesarios para el ecosistema
    public struct DatosTerreno{
        public Vector3[,] centros;
        public bool[,] caminables;
        public bool[,] shore;
        public int[,] indicesTerrenos;
        public DatosTerreno(Vector3[,] cent, bool[,] cam, bool[,] s, int[,] indT){
            this.centros = cent;
            this.caminables = cam;
            this.shore = s;
            this.indicesTerrenos = indT;
        }
    }


}

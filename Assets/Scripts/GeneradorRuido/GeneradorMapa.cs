using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneradorMapa : MonoBehaviour {
    ///<summary>Tipos de dibujo posibles, uno para dibujar a color y otro para blanco y negro</summary>
    public enum DrawMode {NoiseMap, ColourMap};
    public DrawMode modoDibujo;

    //Parametros para dibujar la textura
    //Ancho y largo de la textura
    public int ancho;
    public int largo;
    //Cuanto 'zoom' hacemos sobre el ruido
    public float escaladoRuido;

    //Parametros extra para el ruido

    public int octaves;
    public float lacunaridad;
    [Range(0,1)]
    public float persistencia;
    public int seed;
    public Vector2 offset;
    ///<summary>Tipos de terrenos diferentes para dibujar el mapa</summary>
    public TipoTerreno[] terrenos;

    public bool autoUpdate;

    public void GenerarMapa(){
        //Mapa de ruido
        float[,] mapaRuido = Ruido.GeneradorRuido(ancho, largo, seed, escaladoRuido, octaves, persistencia, lacunaridad, offset);

        //Mapa de colores
        Color[] mapaColores = new Color[ancho * largo];
        for (int y = 0; y < largo; y++) {
            for (int x = 0; x < ancho; x++) {
                float alturaActual = mapaRuido[x,y];
                for (int i = 0; i < terrenos.Length; i++) {
                    //Devolvemos el color dentro del cual este la altura de este punto
                    if(alturaActual <= terrenos[i].altura){
                        mapaColores[y * ancho + x] = terrenos[i].color;
                        break;
                    }
                }
            }
        }      

        //Obtenemos el script MapDisplay que esta en nuestro mismo GameObject
        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
        if(modoDibujo == DrawMode.NoiseMap)
            mapDisplay.DibujarTextura(GeneradorTextura.TextureFromHeigthMap(mapaRuido));
        else if(modoDibujo == DrawMode.ColourMap)
            mapDisplay.DibujarTextura(GeneradorTextura.TextureFromColourMap(mapaColores, largo, ancho));
    }

    //Se llama cada vez que se altera un valor del editor. Aprovechamos para comprobar que haya valores validos
    void OnValidate() {
        if(largo < 1)
            largo = 1;    
        if(ancho < 1)
            ancho = 1;
        if(octaves < 0)
            octaves = 0;
        if(lacunaridad < 1)
            largo = 1;
    }

    //Para que se pueda crear desde el editor
    [System.Serializable]
    public struct TipoTerreno{
        public string nombre;
        public float altura;
        public Color color;
    }
}

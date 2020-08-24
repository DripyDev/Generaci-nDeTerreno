using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator {
    ///<summary>Devuelve una textura2D a partir de un mapa de colores, altura y ancho</summary>
    public static Texture2D TextureFromColourMap(Color[] mapaColor, int altura, int ancho){
        Texture2D textura = new Texture2D(altura, ancho);

        textura.filterMode = FilterMode.Point;//Para definir mejor las fronteras (como antialising)
        textura.wrapMode = TextureWrapMode.Clamp;//Para que los limites de las texturas no sean la del otro lado (de forma predefinida esta en repeat asi que el limite de la textura se ve parte de la del otro lado)

        textura.SetPixels(mapaColor);
        textura.Apply();
        return textura;
    }

    ///<summary>Devuelve una textura2D en color a partir de un mapa de alturas. Devuelve color a partir de ruido</summary>
    public static Texture2D TextureFromHeigthMap(float[,] mapaAlturas){
        int ancho = mapaAlturas.GetLength(0);
        int largo = mapaAlturas.GetLength(1);

        Color[] mapaColores = new Color[ancho * largo];
        for (int y = 0; y < largo; y++){
            for (int x = 0; x < ancho; x++) {
                mapaColores[y * ancho + x] = Color.Lerp(Color.black, Color.white, mapaAlturas[x,y]);
            }
        }
        return TextureFromColourMap(mapaColores, largo, ancho);
    }
}

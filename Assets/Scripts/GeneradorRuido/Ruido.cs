using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>Clase para generar ruido perlin</summary>
public static class Ruido {
    public static float[,] GeneradorRuido(int ancho, int largo, int seed, float escalado, int octaves, float persistencia, float lacunaridad, Vector2 offset){
        float[,] mapaRuido = new float[ancho,largo];
        
        System.Random rnd = new System.Random(seed);
        Vector2[] octaveOffset = new Vector2[octaves];
        for (int i = 0; i < octaves; i++){
            octaveOffset[i] = new Vector2(rnd.Next(-100000, 100000) + offset.x, rnd.Next(-100000, 100000) + offset.y);
        }
        
        if(escalado<=0)
            escalado = 0.0001f;

        float maxAlturaRuido = float.MinValue;
        float minAlturaRuido = float.MaxValue;

        for (int y = 0; y < largo; y++) {
            for (int x = 0; x < ancho; x++) {
                
                float amplitud = 1f;
                float frecuencia = 1f;
                float alturaRuido = 0f;

                for (int i = 0; i < octaves; i++) {
                    //Valores x e y que vamos a usar para el ruido. restamos a x e y la mitad del mapa para que al aumentar el escalado lo haga hacia el centro de la textura
                    float X = (x - (ancho/2f) ) / escalado * frecuencia + octaveOffset[i].x;
                    float Y = (y - (largo/2f)) / escalado * frecuencia + octaveOffset[i].y;

                    //*2-1 es para que tambien pueda dar valores negativos
                    float valorPerling = Mathf.PerlinNoise(X, Y) * 2 - 1;
                    //NOTA: REPASAR COMO FUNCIONAN ESTOS VALORES
                    alturaRuido += valorPerling * amplitud;
                    amplitud *= persistencia;
                    frecuencia *= lacunaridad;
                }
                
                //Como podemos tener valores negativos (ya no solo entre 0 y 1) vamos a almacenar el valor mas alto y mas bajo para normalizarlo despues
                if(alturaRuido > maxAlturaRuido)
                    maxAlturaRuido = alturaRuido;
                else if(alturaRuido < minAlturaRuido)
                    minAlturaRuido = alturaRuido;

                mapaRuido[x,y] = alturaRuido;
            }
        }

        //Como podemos tener valores diferentes de [0,1] vamos a normalizarlos
        for (int y = 0; y < largo; y++) {
            for (int x = 0; x < ancho; x++){
                //Normaliza el valor de mapaRuido[x,y] entre los valores minAlturaRuido y maxAlturaRuido
                mapaRuido[x,y] = Mathf.InverseLerp(minAlturaRuido, maxAlturaRuido, mapaRuido[x,y]);
            }  
        }
         
        return mapaRuido;
    }
}

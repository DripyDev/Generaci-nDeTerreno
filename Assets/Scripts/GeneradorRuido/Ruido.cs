using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>Clase para generar ruido perlin</summary>
public static class Ruido {
    public enum NormalizeMode{Local, Global};
    public static float[,] GeneradorRuido(int ancho, int largo, int seed, float escalado, int octaves, float persistencia, float lacunaridad, Vector2 offset, NormalizeMode normalizeMode){
        float[,] mapaRuido = new float[ancho,largo];
        
        System.Random rnd = new System.Random(seed);
        Vector2[] octaveOffset = new Vector2[octaves];

        float maxPosibleHeigth = 0;
        float amplitud = 1f;
        float frecuencia = 1f;

        for (int i = 0; i < octaves; i++){
            //En Y restamos para que al alterar el offset de forma positiva vaya hacia abajo. En principio no deberia de haber problema porque estuviera al reves
            octaveOffset[i] = new Vector2(rnd.Next(-100000, 100000) + offset.x, rnd.Next(-100000, 100000) - offset.y);

            //Maxima altura posible (multiplicamos por PerlinNoise pero como su maximo valor es 1 pues no hace falta multiplicar xD)
            maxPosibleHeigth += amplitud;
            amplitud *= persistencia;
        }
        
        if(escalado<=0)
            escalado = 0.0001f;

        float maxLocalAlturaRuido = float.MinValue;
        float minLocalAlturaRuido = float.MaxValue;

        for (int y = 0; y < largo; y++) {
            for (int x = 0; x < ancho; x++) {
                
                amplitud = 1f;
                frecuencia = 1f;
                float alturaRuido = 0f;

                for (int i = 0; i < octaves; i++) {
                    //Valores x e y que vamos a usar para el ruido. restamos a x e y la mitad del mapa para que al aumentar el escalado lo haga hacia el centro de la textura
                    //El restado del offset del octave esta dentro para que tambien le afecte el escalado
                    float X = (x - (ancho/2f) + octaveOffset[i].x) / escalado * frecuencia;
                    float Y = (y - (largo/2f) + octaveOffset[i].y) / escalado * frecuencia;

                    //*2-1 es para que tambien pueda dar valores negativos
                    float valorPerling = Mathf.PerlinNoise(X, Y) * 2 - 1;
                    //NOTA: REPASAR COMO FUNCIONAN ESTOS VALORES
                    alturaRuido += valorPerling * amplitud;
                    amplitud *= persistencia;
                    frecuencia *= lacunaridad;
                }
                
                //Como podemos tener valores negativos (ya no solo entre 0 y 1) vamos a almacenar el valor mas alto y mas bajo para normalizarlo despues
                if(alturaRuido > maxLocalAlturaRuido)
                    maxLocalAlturaRuido = alturaRuido;
                else if(alturaRuido < minLocalAlturaRuido)
                    minLocalAlturaRuido = alturaRuido;

                mapaRuido[x,y] = alturaRuido;
            }
        }

        //Es esto lo que causa que los chunks vecinos no esten perfectamente pegados. El problema es que minLocalAlturaRuido y maxLocalAlturaRuido son diferentes en cada chunk
        //Como podemos tener valores diferentes de [0,1] vamos a normalizarlos
        for (int y = 0; y < largo; y++) {
            for (int x = 0; x < ancho; x++){
                if(normalizeMode == NormalizeMode.Local){
                    //Normaliza el valor de mapaRuido[x,y] entre los valores minLocalAlturaRuido y maxLocalAlturaRuido
                    mapaRuido[x,y] = Mathf.InverseLerp(minLocalAlturaRuido, maxLocalAlturaRuido, mapaRuido[x,y]);
                }
                else{
                    //Como dividimos entre maxPosibleHeigth que puede ser muy muy grande, todo quedara pequeño, por eso dividimos entre 1.85 para conseguir valores mas pequeño y normales
                    float normalizedHeight = (mapaRuido[x,y] + 1) / (2f * maxPosibleHeigth / 2f);
                    mapaRuido[x,y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }  
        }
         
        return mapaRuido;
    }
}

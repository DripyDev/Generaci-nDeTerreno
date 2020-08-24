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

        //Queremos que cada octave se muestree de una zona diferente del ruido, por eso el offset
        for (int i = 0; i < octaves; i++){
            //En Y restamos para que al alterar el offset de forma positiva vaya hacia abajo. En principio no deberia de haber problema porque estuviera al reves
            octaveOffset[i] = new Vector2(rnd.Next(-100000, 100000) + offset.x, rnd.Next(-100000, 100000) + offset.y);

            //Maxima altura posible (multiplicamos por PerlinNoise pero como su maximo valor es 1 pues no hace falta multiplicar xD). Para el uso de modo global
            maxPosibleHeigth += amplitud;
            amplitud *= persistencia;
        }
        
        if(escalado<=0)
            escalado = 0.0001f;

        float maxLocalAlturaRuido = float.MinValue;
        float minLocalAlturaRuido = float.MaxValue;

        for (int y = 0; y < largo; y++) {
            for (int x = 0; x < ancho; x++) {
                
                //Reduce la altura de cada octava por lo que cada una tendra mayores detalles porque tendremos valores en un menor rango?¿?
                //va en funcion de la persistencia, a mayor valor, mas granular sera cuando se sumen octavas, a menor, menos granular
                amplitud = 1f;
                //Es aproximadamente el 'zoom' que hacemos sobre el ruido en cada octave. Cada vez va a mas para mejorar los detalles al acumular octavas
                frecuencia = 1f;
                float alturaRuido = 0f;

                for (int i = 0; i < octaves; i++) {
                    //Valores x e y que vamos a usar para el ruido. restamos a x e y la mitad del mapa para que al aumentar el escalado lo haga hacia el centro de la textura
                    //El restado del offset del octave esta dentro para que tambien le afecte el escalado. Aunque en principio no pasa nada porque este fuera
                    float X = (x - (ancho/2f) ) / escalado * frecuencia+ octaveOffset[i].x;
                    float Y = (y - (largo/2f) ) / escalado * frecuencia+ octaveOffset[i].y;

                    //*2-1 es para que tambien pueda dar valores negativos lo que hara que alturaRuido pueda reducirse lo que hara que haya ruido mas interesante
                    float valorPerling = Mathf.PerlinNoise(X, Y) * 2 - 1;
                    //La altura que vamos acumulando de juntar diferentes capas de ruido
                    alturaRuido += valorPerling * amplitud;
                    //Reducimos la amplitud por lo que los posibles valores seran reducidos
                    amplitud *= persistencia;
                    //Aumentamos la frecuencia lo que se traduce en un 'zoom' sobre la textura
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

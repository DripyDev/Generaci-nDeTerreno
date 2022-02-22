# Generacion de terreno
Vamos a generar terrenos de manera procedural. Esto puede ser separado en tres secciones diferentes: **generación del mesh**, **generación de ruido y
mapa de colores** y **la generación del terreno final**. Los objetivos finales son dos, crear un terreno semi-realista de manera procedural con montañas, mares etc y convertir este terreno a uno con una temática de casillas. Las siguientes imagenes muestran los resultados finales:


Terreno final |  Terreno con temáticas de casillas
:-------------------------:|:-------------------------:
![](https://user-images.githubusercontent.com/61519721/142904805-f7290eb6-7206-44f0-b7df-cf2a4dde6d0c.PNG)  |  ![](https://user-images.githubusercontent.com/61519721/142904802-de8679c3-3009-42b6-9dbb-8d8a53b8a0f7.PNG)

Los pasos a seguir para generar el terreno son los siguientes:
1. Generar un mesh
2. Crear un mapa de ruido
3. Modificar la altura (y colores) de los vértices del mesh con el mapa de ruido

## Generación del mesh
Lo primero que debemos hacer es crear un mesh, una estructura cuyos vértices modificarémos. Este mesh contiene la información de las **posiciones de sus vértices**, **los índices de los triángulos** que componen las caras del mesh, las **coordenadas uv** de la textura y los **vectores normales** de las caras. Las coordenadas uv no nos interesan de momento. Los índices de los triángulos que forman las caras del mesh tienen que seguir un sentido horario, característica de Unity. Teniendo esto en cuenta, podemos empezar a generar un mesh de longitud N.

![Mesh](https://user-images.githubusercontent.com/61519721/155167002-21225b33-9eb9-4713-b5f3-530a1f134ca1.PNG)

## Generación de ruido y mapa de colores
Con el mesh generado, vamos a crear un mapa de alturas a través del **ruido Perlin**. Esto va a generar un mapa con valores entre 0 y 1 que podemos convertir a colores en función de rangos que predefinamos o a altura de su correspondiente vértice.

Ruido Perlin |  Mapa de colores
:-------------------------:|:-------------------------:
![](https://user-images.githubusercontent.com/61519721/142911369-dadcb3b2-8753-4cdb-91dd-2d78c5e27688.PNG)  |  ![](https://user-images.githubusercontent.com/61519721/142911371-482d28c2-01fd-430d-8ef3-0799b03b749f.PNG)

Cabe destacar que a la hora de generar el ruido podemos hacerlo simplemente con la función *PerlinNoise* del paquete *Mathf* de Unity o podemos mejorarlo de manera significativa aplicando una serie de técnicas. A continuación podemos ver la diferencia en el ruido sin y con mejoras tanto en mapa de alturas como en mapa de colores.

Ruido Perlin no mejorado |  Ruido Perlin mejorado
:-------------------------:|:-------------------------:
![](https://user-images.githubusercontent.com/61519721/142911059-f263652f-c893-4796-9911-0c2639ba7b5d.PNG)  |  ![](https://user-images.githubusercontent.com/61519721/142911069-33af3d0c-84c1-4e5f-9f21-ca517def4d87.PNG)

Mapa de colores no mejorado |  Mapa de colores mejorado
:-------------------------:|:-------------------------:
![](https://user-images.githubusercontent.com/61519721/142911066-770cf526-e0a3-4f05-b46d-7dcd90588471.PNG)  |  ![](https://user-images.githubusercontent.com/61519721/142911063-f5984174-7e4a-4802-9acd-d899759aa00a.PNG)

Las mejoras son a través del uso de **octavas**, una **semilla**, **persistencia** y **lagunaridad**. A continuación resumiré de manera breve para que sirve cada una de las mejoras.

+ **Octavas:** Para obtener mas detalle acumulamos capas (octavas) unas sobre otras, pero cada una partirá de un origen diferente. A mas capas, mayor detalle, aunque esto depende en gran medida del valor de la persistencia, que afecta al rango de valores que se pueden tomar.
+ **Persistencia:** Afecta al rango de valores que puede tomar el mapa de alturas. Al limitar los valores que puede tomar el mapa de alturas creamos un mayor efecto de "granularidad".
+ **Lagunaridad:** Aumentar el valor equivale a un "zoom" sobre el ruido generado, esto acumulado sobre las múltiples capas de las octavas da mayor detalle al resultado final.
+ **Semilla:** La generación procedural conlleva un cierto grado de aleatoriedad que parte de un número subjetivo que llamamos "semilla". Conviene cambiar este número cada vez que generamos un terreno para obtener resultados significativamente diferentes cada vez.


## Generación del terreno final

Con el **mapa de colores** y de **altura** junto al mesh, para generar el terreno final basta con alterar la altura de los vértices en función del segundo mapa y sus colores en función del primero. Una rápida mejora que podemos hacer es utilizar una curva para aplanar los puntos correspondientes al mar y que tengan la misma altura, para ello podemos utilizar un tipo de dato llamado [*AnimationCurve*](https://docs.unity3d.com/ScriptReference/AnimationCurve.html) que el propio Unity nos da.

Terreno sin curva |  Terreno con curva
:-------------------------:|:-------------------------:
![](https://user-images.githubusercontent.com/61519721/155173294-9125722a-326d-421f-81fa-463f70fae015.PNG)  |  ![](https://user-images.githubusercontent.com/61519721/155173302-2602cf0a-7925-4335-ada1-a0ec2d170c54.PNG)


--------------------------------------------------
Si quisieramos simular un terreno mas similar a una isla, podemos utilizar un **mapa falloff** generado con la siguiente función matemática:

![FormullaFalloff](https://user-images.githubusercontent.com/61519721/155172769-d2caa246-6050-4345-9ac8-fcd6bce26392.PNG)

Donde **a=3** y **b=2.2**.

Terreno sin falloff |  Terreno con falloff
:-------------------------:|:-------------------------:
![](https://user-images.githubusercontent.com/61519721/155172936-c40a56aa-224f-40ba-a022-0b2481f8f254.PNG)  |  ![](https://user-images.githubusercontent.com/61519721/155172928-c59e221a-8f16-4a10-9968-c391b7c77dfd.PNG)

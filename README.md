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

![Mesh](![MeshPlano](https://user-images.githubusercontent.com/61519721/142908109-82b6bbaa-fe72-44eb-9d17-d8c17785e23c.PNG))

## Generación de ruido y mapa de colores
Con el mesh generado, vamos a crear un mapa de alturas a través del **ruido Perlin**. Esto va a generar un mapa con valores entre 0 y 1 que podemos convertir a colores en función de rangos que predefinamos o a altura de su correspondiente vértice.

Ruido Perlin |  Mapa de colores
:-------------------------:|:-------------------------:
![](https://user-images.githubusercontent.com/61519721/142904805-f7290eb6-7206-44f0-b7df-cf2a4dde6d0c.PNG)  |  ![](https://user-images.githubusercontent.com/61519721/142904802-de8679c3-3009-42b6-9dbb-8d8a53b8a0f7.PNG)

Cabe destacar que a la hora de generar el ruido podemos hacerlo simplemente con la función *PerlinNoise* del paquete *Mathf* de Unity o podemos mejorarlo de manera significativa aplicando una serie de técnicas. A continuación podemos ver la diferencia en el ruido sin y con mejoras tanto en mapa de alturas como en mapa de colores.

Ruido Perlin no mejorad |  Ruido Perlin mejorado
:-------------------------:|:-------------------------:
![](https://user-images.githubusercontent.com/61519721/142904805-f7290eb6-7206-44f0-b7df-cf2a4dde6d0c.PNG)  |  ![](https://user-images.githubusercontent.com/61519721/142904802-de8679c3-3009-42b6-9dbb-8d8a53b8a0f7.PNG)

Mapa de colores no mejorado |  Mapa de colores mejorado
:-------------------------:|:-------------------------:
![](https://user-images.githubusercontent.com/61519721/142904805-f7290eb6-7206-44f0-b7df-cf2a4dde6d0c.PNG)  |  ![](https://user-images.githubusercontent.com/61519721/142904802-de8679c3-3009-42b6-9dbb-8d8a53b8a0f7.PNG)

## Generación del terreno final

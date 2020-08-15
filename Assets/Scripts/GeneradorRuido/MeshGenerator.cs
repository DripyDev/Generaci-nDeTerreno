using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator {
    public static MeshData GenerateTerrainMesh(float[,] mapaAlturas, float multiplicadorAltura, AnimationCurve curvaAltura){
        int ancho = mapaAlturas.GetLength(0);
        int largo = mapaAlturas.GetLength(1);
        float topLeftX = (ancho-1) / -2f;
        float topLeftZ = (largo-1) / 2f;

        MeshData meshData = new MeshData(ancho, largo);
        int indiceVertices = 0;

        for (int y = 0; y < largo; y++) {
            for (int x = 0; x < ancho; x++) {
                meshData.vertices[indiceVertices] = new Vector3(topLeftX + x, curvaAltura.Evaluate(mapaAlturas[x,y]) * multiplicadorAltura, topLeftZ - y);
                meshData.uvs[indiceVertices] = new Vector2(x/(float) ancho, y/(float)largo);

                if(x < ancho -1 && y < largo -1){
                    //Los dos triangulos generados a partir de 4 vertices
                    meshData.AñadirTriangulo(indiceVertices, indiceVertices + ancho +1, indiceVertices + ancho);
                    meshData.AñadirTriangulo(indiceVertices + ancho +1, indiceVertices, indiceVertices + 1);
                }

                indiceVertices++;
            }
        }
        return meshData;
    }

}

//Clase para almacenar la informacion necesaria para el mesh
public class MeshData{
    public Vector3[] vertices;
    public int[] triangulos;
    public Vector2[] uvs;

    int indiceTriangulo;

    public MeshData(int anchoMesh, int largoMesh){
        vertices = new Vector3[anchoMesh * largoMesh];
        triangulos = new int[(anchoMesh-1) * (largoMesh-1) * 6];
        //uv-s para generar de manera correcta las texturas
        uvs = new Vector2[anchoMesh * largoMesh];
    }

    public void AñadirTriangulo(int a, int b, int c){
        triangulos[indiceTriangulo] = a;
        triangulos[indiceTriangulo + 1] = b;
        triangulos[indiceTriangulo + 2] = c;
        indiceTriangulo += 3;
    }

    public Mesh CrearMesh(){
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangulos;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}

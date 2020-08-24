using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {
    public Renderer texturaRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public void DibujarTextura(Texture2D textura){
        texturaRender.sharedMaterial.mainTexture = textura;
        texturaRender.transform.localScale = new Vector3(textura.width, 1, textura.height);
    }

    public void DibujarMesh(MeshData meshData, Texture2D textura){
        meshFilter.sharedMesh = meshData.CrearMesh();
        meshRenderer.sharedMaterial.mainTexture = textura;
    }

    public void DibujarMesh2(Mesh mesh, Texture2D textura){
        meshFilter.sharedMesh = mesh;
        meshRenderer.sharedMaterial.mainTexture = textura;
    }
}

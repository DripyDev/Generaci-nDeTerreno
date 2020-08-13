using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {
    public Renderer texturaRender;

    public void DibujarTextura(Texture2D textura){
        texturaRender.sharedMaterial.mainTexture = textura;
        texturaRender.transform.localScale = new Vector3(textura.width, 1, textura.height);
    }
}

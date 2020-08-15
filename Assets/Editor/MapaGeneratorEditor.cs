using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GeneradorMapa))]
public class MapaGeneratorEditor : Editor {
    public override void OnInspectorGUI(){
        GeneradorMapa mapGen = (GeneradorMapa)target;

        if(DrawDefaultInspector()){
            if(mapGen.autoUpdate){
                mapGen.DrawMapInEditor();
            }
        }

        if(GUILayout.Button("Generate"))
            mapGen.DrawMapInEditor();
    }
}

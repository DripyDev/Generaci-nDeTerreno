using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GeneradorTerreno))]
public class EditorGT : Editor {
    public override void OnInspectorGUI(){
        GeneradorTerreno mapGen = (GeneradorTerreno)target;

        if(DrawDefaultInspector()){
            if(mapGen.autoUpdate){
                mapGen.DrawMapInEditor();
            }
        }

        if(GUILayout.Button("Generate"))
            mapGen.DrawMapInEditor();
    }
}

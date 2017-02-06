using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlantUMLConvertWindow : EditorWindow {

    [MenuItem("UML/ConvertWindow")]
    static private void CreateWindow()
    {
        var window = EditorWindow.GetWindow<PlantUMLConvertWindow> ();

        window.titleContent = new GUIContent ("PlantUMLConverter");
        window.Show ();
    }

    private void OnGUI() {
        GUILayout.Label ("対象クラス図");

    }
}


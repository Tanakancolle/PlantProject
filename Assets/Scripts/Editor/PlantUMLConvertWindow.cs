using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlantUMLConvertWindow : EditorWindow {

    private static PlantUMLConvertWindow umlWindow;

    [MenuItem("UML/ConvertWindow")]
    static private void CreateWindow()
    {
        if (umlWindow == null) {
            umlWindow = CreateInstance<PlantUMLConvertWindow> ();
        }

        umlWindow.titleContent = new GUIContent ("PlantUMLConverter");
        umlWindow.ShowUtility ();
    }

    private TextAsset textAsset;
    private string createPath = "Assets/";

    private void OnGUI() {
        EditorGUILayout.BeginHorizontal ();
        EditorGUILayout.LabelField ("対象クラス図", GUILayout.MaxWidth (100));
        textAsset = EditorGUILayout.ObjectField (textAsset, typeof(TextAsset), false) as TextAsset;
        EditorGUILayout.EndHorizontal ();

        EditorGUILayout.BeginHorizontal ();
        EditorGUILayout.LabelField ("生成フォルダ", GUILayout.MaxWidth (100));
        createPath = EditorGUILayout.TextField (createPath);
        EditorGUILayout.EndHorizontal ();

        if (GUILayout.Button ("生成開始")) {
            var converter = new PlantUMLConverter ();
            converter.ConvertProcess (textAsset.text, createPath);
        }
    }
}


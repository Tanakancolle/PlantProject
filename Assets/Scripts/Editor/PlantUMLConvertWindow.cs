using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// PlantUML変換ウィンドウ
/// </summary>
public class PlantUMLConvertWindow : EditorWindow {

    /// <summary>
    /// インスタンス
    /// </summary>
    private static PlantUMLConvertWindow umlWindow;

    /// <summary>
    /// ウィンドウ生成
    /// </summary>
    [MenuItem("UML/ConvertWindow")]
    static private void CreateWindow()
    {
        if (umlWindow == null) {
            umlWindow = CreateInstance<PlantUMLConvertWindow> ();
        }

        umlWindow.titleContent = new GUIContent ("PlantUMLConverter");
        umlWindow.ShowUtility ();
    }

    /// <summary>
    /// クラス図を受け取るテキストアセット
    /// </summary>
    private TextAsset textAsset;

    /// <summary>
    /// 生成するパス
    /// </summary>
    private string createPath = "Assets/";

    /// <summary>
    /// オプション
    /// </summary>
    private PlantUMLConvertOption convertOption;

    private void OnGUI() 
    {
        if (convertOption == null) {
            convertOption = ScriptableObject.CreateInstance<PlantUMLConvertOption> ();
        }

        // 対象クラス
        EditorGUILayout.BeginHorizontal ();
        {
            EditorGUILayout.LabelField ("対象クラス図", GUILayout.MaxWidth (100));
            textAsset = EditorGUILayout.ObjectField (textAsset, typeof(TextAsset), false) as TextAsset;
        }
        EditorGUILayout.EndHorizontal ();

        // 生成フォルダ
        EditorGUILayout.BeginHorizontal ();
        {
            EditorGUILayout.LabelField ("生成フォルダ", GUILayout.MaxWidth (100));
            createPath = EditorGUILayout.TextField (createPath);
        }
        EditorGUILayout.EndHorizontal ();

        // オプション
        EditorGUILayout.BeginVertical (GUI.skin.box);
        {
            EditorGUILayout.LabelField ("オプション");
            //convertOption.arrowPattern = EditorGUILayout.TextField ("矢印パターン", convertOption.arrowPattern);
            //convertOption.arrowExtensionLeftPattern = EditorGUILayout.TextField ("左継承矢印パターン", convertOption.arrowExtensionLeftPattern);
            //convertOption.arrowExtensionRightPattern = EditorGUILayout.TextField ("右継承矢印パターン", convertOption.arrowExtensionRightPattern);

            EditorGUILayout.BeginHorizontal (GUI.skin.box);
            {
                if (GUILayout.Button ("設定保存")) {
                    var save_path = EditorUtility.SaveFilePanel ("", "Assets", "", "asset");
                    SaveOption (save_path);
                }

                if (GUILayout.Button ("設定読み込み")) {
                    var load_path = EditorUtility.OpenFilePanel ("", "Assets", "asset");
                    LoadOption (load_path);
                }
            } 
            EditorGUILayout.EndHorizontal ();
        }
        EditorGUILayout.EndVertical ();

        if (GUILayout.Button ("生成開始")) {
            var converter = new PlantUMLConverter ();
            converter.ConvertProcess (textAsset.text, createPath, convertOption);
        }
    }

    /// <summary>
    /// 設定保存
    /// </summary>
    private void SaveOption(string path) 
    {
        if (string.IsNullOrEmpty (path)) {
            return;
        }

        AssetDatabase.CreateAsset (convertOption, path.Substring (path.IndexOf ("Assets")));
        AssetDatabase.SaveAssets ();

        convertOption = convertOption.Copy ();
    }

    /// <summary>
    /// 設定読み込み
    /// </summary>
    private void LoadOption(string path)
    {
        if (string.IsNullOrEmpty (path)) {
            return;
        }

        var option = AssetDatabase.LoadAssetAtPath<PlantUMLConvertOption> (path.Substring (path.IndexOf ("Assets")));
        if (option == null) {
            return;
        }

        convertOption = option.Copy ();
    }
}


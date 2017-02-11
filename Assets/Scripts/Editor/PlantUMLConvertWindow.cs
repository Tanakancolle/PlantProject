using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

/// <summary>
/// PlantUML変換ウィンドウ
/// </summary>
public class PlantUMLConvertWindow : EditorWindow
{ 
    /// <summary>
    /// インスタンス
    /// </summary>
    private static PlantUMLConvertWindow umlWindow;

    /// <summary>
    /// ウィンドウ生成
    /// </summary>
    [MenuItem ("UML/ConvertWindow")]
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
    /// オプション
    /// </summary>
    private PlantUMLConvertOption convertOption;

    /// <summary>
    /// オプション用シリアライズオブジェクト
    /// </summary>
    private SerializedObject optionSerializedObject;

    /// <summary>
    /// usingリスト表示用
    /// </summary>
    private ReorderableList usingReorderableList;                 

    private void OnGUI()
    {
        // インスタンスチェック
        CheckInstance ();

        // 対象クラス図
        EditorGUILayout.BeginHorizontal ();
        {
            EditorGUILayout.LabelField ("対象クラス図", GUILayout.MaxWidth (100));
            textAsset = EditorGUILayout.ObjectField (textAsset, typeof (TextAsset), false) as TextAsset;
        }
        EditorGUILayout.EndHorizontal ();                  

        // オプション
        EditorGUILayout.BeginVertical (GUI.skin.box);
        { 
            EditorGUILayout.LabelField ("オプション");
            convertOption.createFolderPath = EditorGUILayout.TextField ("生成フォルダ", convertOption.createFolderPath);
            convertOption.arrowPattern = EditorGUILayout.TextField ("矢印パターン", convertOption.arrowPattern);
            convertOption.arrowExtensionLeftPattern = EditorGUILayout.TextField ("左継承矢印パターン", convertOption.arrowExtensionLeftPattern);
            convertOption.arrowExtensionRightPattern = EditorGUILayout.TextField ("右継承矢印パターン", convertOption.arrowExtensionRightPattern);
            convertOption.isNonCreateMember = EditorGUILayout.Toggle ("メンバの非生成", convertOption.isNonCreateMember);

            // using配列設定
            optionSerializedObject.Update ();
            usingReorderableList.DoLayoutList ();
            optionSerializedObject.ApplyModifiedProperties ();
            
            // オプション保存＆読み込み処理
            EditorGUILayout.BeginHorizontal (GUI.skin.box);
            {
                if (GUILayout.Button ("オプション保存")) {
                    var save_path = EditorUtility.SaveFilePanel ("", "Assets", "", "asset");
                    SaveOption (save_path);
                }

                if (GUILayout.Button ("オプション読み込み")) {
                    var load_path = EditorUtility.OpenFilePanel ("", "Assets", "asset");
                    LoadOption (load_path);
                }
            }
            EditorGUILayout.EndHorizontal ();
        }
        EditorGUILayout.EndVertical ();

        // スクリプト生成開始
        if (GUILayout.Button ("生成開始")) {
            if( textAsset == null ) {
                Debug.LogError("クラス図が指定されていません");
                return;
            }

            var converter = new PlantUMLConverter ();
            converter.ConvertProcess (textAsset.text, convertOption);
        }
    }

    /// <summary>
    /// インスタンスをチェック
    /// </summary>
    /// <memo>なぜか消えてしまう事があるため毎回チェック</memo>
    private void CheckInstance()
    {
        if (convertOption == null) {
            convertOption = ScriptableObject.CreateInstance<PlantUMLConvertOption> ();
        }

        if (optionSerializedObject == null) {
            optionSerializedObject = new SerializedObject (convertOption);
            SetupDeclarationUsingOption ();
        }

    }

    /// <summary>
    /// 宣言usingオプションセットアップ
    /// </summary>
    private void SetupDeclarationUsingOption()
    {
        var property = optionSerializedObject.FindProperty ("declarationUsings");

        var reorderable = new ReorderableList (optionSerializedObject, property);
        reorderable.drawElementCallback = (rect, index, isActive, isFocused) => {
            var element = property.GetArrayElementAtIndex (index);
            rect.height -= 4;
            rect.y += 2;
            EditorGUI.PropertyField (rect, element);
        };

        reorderable.drawHeaderCallback = (rect) => EditorGUI.LabelField (rect, "宣言するusing");

        usingReorderableList = reorderable;
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

        // usignオプション表示準備       
        optionSerializedObject = new SerializedObject (convertOption);
        SetupDeclarationUsingOption ();
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

        // usignオプション表示準備         
        optionSerializedObject = new SerializedObject (convertOption);
        SetupDeclarationUsingOption ();
    }
}

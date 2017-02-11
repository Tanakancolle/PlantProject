using UnityEngine;

/// <summary>
/// PlantUML変換オプション
/// </summary>
public class PlantUMLConvertOption : ScriptableObject 
{
    /// <summary>
    /// 生成フォルダパス
    /// </summary>
    public string createFolderPath = "Assets/";

    /// <summary>
    /// 矢印パターン
    /// </summary>
    public string arrowPattern = @"(?:<\|{dir}-{1,}|-{1,}{dir}\|>|<-{dir}-{1,}|-{1,}{dir}->)";

    /// <summary>
    /// 拡張左矢印パターン
    /// </summary>
    public string arrowExtensionLeftPattern = @"(?:<\|{dir}-{1,})";

    /// <summary>
    /// 拡張右矢印パターン
    /// </summary>
    public string arrowExtensionRightPattern = @"(?:-{1,}{dir}\|>)";

    /// <summary>
    /// コピー
    /// </summary>
    public PlantUMLConvertOption Copy() 
    {
        var option = ScriptableObject.CreateInstance<PlantUMLConvertOption> ();

        option.arrowPattern = arrowPattern;
        option.arrowExtensionLeftPattern = arrowExtensionLeftPattern;
        option.arrowExtensionRightPattern = arrowExtensionRightPattern;

        return option;
    }
}

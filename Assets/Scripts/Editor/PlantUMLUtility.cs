using System.Collections.Generic;


/// <summary>
/// PlantUML系スクリプトの共有クラス
/// </summary>
public static class PlantUMLUtility 
{
    /// <summary>
    /// 方向パターン
    /// </summary>
    private const string dirPattern = @"(?:|right|left|up|down|r|l|u|d)";

    /// <summary>
    /// アクセス修飾子テーブル
    /// </summary>
    private static readonly Dictionary<string,string> accessModifiersTable = new Dictionary<string, string>() {
        {"+", "public "},  
        {"-", "private " },
        {"#", "protected " }
    };

    /// <summary>
    /// 方向パターン置き換え
    /// </summary>
    /// <returns>置き換え後文字列</returns>
    public static string ReplaceDirPattern( string text )
    {
        return text.Replace ("{dir}", PlantUMLUtility.dirPattern);
    }

    /// <summary>
    /// アクセス修飾子置き換え
    /// </summary>
    /// <returns>置き換え後文字列</returns>
    /// <param name="line">文字列</param>
    public static string ReplaceAccessModifiers( string line )
    {
        foreach (var replace in PlantUMLUtility.accessModifiersTable) {
            if (line.IndexOf (replace.Key) >= 0) {
                return line.Replace (replace.Key, replace.Value);
            }
        }

        // アクセス修飾子がない場合はそのまま返す
        return line;
    }

    /// <summary>
    /// スペース毎に分割
    /// </summary>
    public static string[] SplitSpace( string line ) 
    {
        return line.Split (' ');
    }

    /// <summary>
    /// 同ワードチェック
    /// </summary>
    /// <returns><c>true</c>, ワード有り, <c>false</c> ワード無し.</returns>
    /// <param name="words">ワード配列</param>
    /// <param name="check_word">チェックワード</param>
    public static bool CheckContainsWords(string[] words, string check_word) 
    {
        foreach (var word in words) {
            if (word.Contains (check_word)) {
                return true;
            }
        }

        return false;
    }
}

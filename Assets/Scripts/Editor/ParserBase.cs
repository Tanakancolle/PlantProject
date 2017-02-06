using System.Collections.Generic;

public abstract class ParserBase {
    /// <summary>
    /// アクセス修飾子テーブル
    /// </summary>
    private static readonly Dictionary<string,string> kACCESS_MODIFIERS_TABLE = new Dictionary<string, string>() {
        {"+", "public "},  
        {"-", "private " },
        {"#", "protected " }
    };

    public abstract StructuralInfoBase[] Parse (string[] lines, ref int index, string namespace_name = "");

    /// <summary>
    /// スペース毎に分割
    /// </summary>
    protected string[] SplitSpace( string line ) {
        return line.Split (' ');
    }

    /// <summary>
    /// ワードチェック
    /// </summary>
    protected bool CheckWord(string[] words, string check_word) {
        foreach (var word in words) {
            if (word.Contains (check_word)) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// アクセス修飾子置き換え
    /// </summary>
    /// <returns>置き換え後文字列</returns>
    /// <param name="line">文字列</param>
    protected string ReplaceAccessModifiers( string line )
    {
        foreach (var replace in kACCESS_MODIFIERS_TABLE) {
            if (line.IndexOf (replace.Key) >= 0) {
                return line.Replace (replace.Key, replace.Value);
            }
        }

        return line;
    }
} 
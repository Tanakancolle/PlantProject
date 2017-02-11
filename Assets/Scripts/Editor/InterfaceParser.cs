using System.Collections.Generic;

/// <summary>
/// インターフェースパーサー
/// </summary>
public class InterfaceParser : IContentParser
{
    public ContentInfoBase[] Parse (string[] lines, ref int index, string namespace_name = "")
    {
        var words = PlantUMLUtility.SplitSpace (lines [index]);

        // インターフェースチェック
        if (!PlantUMLUtility.CheckContainsWords (words, "interface")) {
            return null;
        }
        
        var info = new InterfaceInfo ();

        // インターフェース名設定
        info.SetName (lines [index].Replace ("interface", string.Empty).Replace ("{", string.Empty).Trim ());

        // 内容までインデックスをずらす
        index++;
        if (lines [index].IndexOf ("{") >= 0) {
            index++;
        }

        // 定義終了まで内容をパース
        while (lines [index].IndexOf ("}") < 0) {
            var member = new MemberInfo ();

            member.name = string.Format ("public {0}", lines [index].TrimStart ());
            member.isAbstract = true;
             
            if (!string.IsNullOrEmpty (member.name)) {
                info.AddMemberInfo (member);
            }

            index++;
        }

        index++;

        return new ContentInfoBase[] { info };
    }
}

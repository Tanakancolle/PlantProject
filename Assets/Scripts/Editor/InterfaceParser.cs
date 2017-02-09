using System.Collections.Generic;

/// <summary>
/// インターフェースパーサー
/// </summary>
public class InterfaceParser : IContentParser {
    public ContentInfoBase[] Parse (string[] lines, ref int index,string namespace_name = "")
    {
        var words = PlantUMLUtility.SplitSpace (lines [index]);

        // インターフェースチェック
        if (!PlantUMLUtility.CheckContainsWords (words, "interface")) {
            return null;
        }
        
        var info = new InterfaceInfo ();

        // インターフェース名設定
        info.contentName = lines [index].Replace ("interface", string.Empty).Replace ("{", string.Empty).Trim ();

        // 内容までインデックスをずらす
        index++;
        if (lines [index].IndexOf ("{") >= 0) {
            index++;
        }

        // 定義終了まで内容をパース
        info.memberList = new List<MemberInfo> ();
        while (lines [index].IndexOf ("}") < 0) {
            var member = new MemberInfo ();

            member.name = string.Format ("public {0}", lines [index].TrimStart ());
            member.isAbstract = true;

            info.memberList.Add (member);

            index++;
        }

        index++;

        return new ContentInfoBase[] { info };
    }
}

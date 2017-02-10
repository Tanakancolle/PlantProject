using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnumParser : IContentParser {
    public ContentInfoBase[] Parse (string[] lines, ref int index, string namespace_name = "")
    {
        var words = PlantUMLUtility.SplitSpace (lines [index]);

        // 列挙型チェック
        if (!PlantUMLUtility.CheckContainsWords (words, "enum")) {
            return null;
        }

        var info = new EnumInfo ();

        // 名前設定
        info.SetName(lines[index].Replace("enum", string.Empty).Replace("{",string.Empty).Trim());

        // 内容までインデックスをずらす
        index++;
        if (lines [index].IndexOf ("{") >= 0) {
            index++;
        }

        // 定義終了まで内容をパース
        while (lines [index].IndexOf ("}") < 0) {
            var member = new MemberInfo ();

            member.name = lines [index].TrimStart ();
            info.AddMemberInfo (member);

            index++;
        }

        index++;

        return new ContentInfoBase[] { info };
    }
}

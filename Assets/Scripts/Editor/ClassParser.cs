using System.Collections.Generic;

/// <summary>
/// クラスパーサー
/// </summary>
public class ClassParser : IContentParser
{
    public ContentInfoBase[] Parse (string[] lines, ref int index, string namespace_name = "")
    {
        var words = PlantUMLUtility.SplitSpace (lines [index]);
        
        // クラスチェック
        if (!PlantUMLUtility.CheckContainsWords (words, "class")) {
            return null;
        }

        var info = new ClassInfo ();

        // ネームスペース設定
        info.SetNamespace (namespace_name);

        // クラス名設定
        info.SetName (lines [index].Replace ("class", string.Empty).Replace ("{", string.Empty).Replace ("abstract", string.Empty).Trim ());

        // 抽象クラスフラグ設定
        info.isAbstract = PlantUMLUtility.CheckContainsWords (PlantUMLUtility.SplitSpace (lines [index]), "abstract");

        // 内容までインデックスをずらす
        index++;
        if (lines [index].IndexOf ("{") >= 0) {
            index++;
        }

        // 定義終了まで内容をパース
        while (lines [index].IndexOf ("}") < 0) {
            var member = new MemberInfo ();

            member.name = PlantUMLUtility.ReplaceAccessModifiers (lines [index]).TrimStart ();
            member.isAbstract = PlantUMLUtility.CheckContainsWords (PlantUMLUtility.SplitSpace (lines [index]), "abstract");

            if (!string.IsNullOrEmpty (member.name)) {
                info.AddMemberInfo (member);
            }

            index++;
        }

        return new ContentInfoBase[] { info };
    }
}

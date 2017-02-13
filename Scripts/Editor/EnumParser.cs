
namespace UML
{

    public class EnumParser : IContentParser
    {
        public ContentInfoBase[] Parse(string[] lines, ref int index, string namespace_name = "")
        {
            var words = PlantUMLUtility.SplitSpace (lines[index]);

            // 列挙型チェック
            if (!PlantUMLUtility.CheckContainsWords (words, "enum")) {
                return null;
            }

            var info = new EnumInfo ();

            // ネームスペース設定
            info.SetNamespace (namespace_name);

            // 名前設定
            info.SetName (lines[index].Replace ("enum", string.Empty).Replace ("{", string.Empty).Trim ());

            // 内容までインデックスをずらす
            index++;
            if (lines[index].IndexOf ("{") >= 0) {
                index++;
            }

            // 定義終了まで内容をパース
            while (lines[index].IndexOf ("}") < 0) {
                var member = new MemberInfo ();

                member.name = lines[index].TrimStart ();

                if (!string.IsNullOrEmpty (member.name)) {
                    info.AddMemberInfo (member);
                }

                index++;
            }

            return new ContentInfoBase[] { info };
        }
    }
}

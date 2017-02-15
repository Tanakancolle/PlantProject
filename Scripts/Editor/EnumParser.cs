
namespace UML
{

    public class EnumParser : IContentParser
    {
        public ContentInfoBase[] Parse(string[] lines, ref int index, string namespace_name = "")
        {
            var words = PlantUMLUtility.SplitSpace (lines[index]);

            // コンテンツ名インデックス取得
            int name_index = PlantUMLUtility.GetContentNameIndexFromWords (words, "enum");
            if (name_index == -1) {
                return null;
            }

            var info = new EnumInfo ();

            // コンテンツ名設定
            info.SetName (words[name_index].Replace ("{", string.Empty));

            // ネームスペース設定
            info.SetNamespace (namespace_name);

            // 内容チェック
            if (lines[index].IndexOf ("{") < 0) {
                return new ContentInfoBase[] { info };
            }

            index++;                            

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

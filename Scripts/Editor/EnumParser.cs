
namespace UML
{

    public class EnumParser : IContentParser
    {
        public IContentBuilder[] Parse(string[] lines, ref int index, string namespace_name = "")
        {
            var words = PlantUMLUtility.SplitSpace (lines[index]);

            // コンテンツ名インデックス取得
            int name_index = PlantUMLUtility.GetContentNameIndexFromWords (words, "enum");
            if (name_index == -1) {
                return null;
            }

            var builder = new EnumBuilder ();

            // コンテンツ名設定
            builder.SetName (words[name_index].Replace ("{", string.Empty));

            // ネームスペース設定
            builder.SetNamespace (namespace_name);

            // 内容チェック
            if (lines[index].IndexOf ("{") < 0) {
                return new IContentBuilder[] { builder };
            }

            index++;                            

            // 定義終了まで内容をパース
            while (lines[index].IndexOf ("}") < 0) {
                var member = lines[index].TrimStart ();

                if (!string.IsNullOrEmpty (member)) {
                    builder.AddMember (member);
                }

                index++;
            }

            return new IContentBuilder[] { builder };
        }
    }
}

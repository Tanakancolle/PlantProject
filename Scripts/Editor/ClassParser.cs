using System.Text.RegularExpressions;


namespace UML
{
    /// <summary>
    /// クラスパーサー
    /// </summary>
    public class ClassParser : IContentParser
    {
        public IContentBuilder[] Parse(string[] lines, ref int index, string namespace_name = "")
        {
            var words = PlantUMLUtility.SplitSpace (lines[index]);

            // コンテンツ名インデックス取得
            int name_index = PlantUMLUtility.GetContentNameIndexFromWords (words, "class");
            if( name_index == -1) {
                return null;
            }

            var builder = new ClassBuilder ();

            // コンテンツ名設定
            builder.SetName (words[name_index].Replace ("{", string.Empty));

            // ネームスペース設定
            builder.SetNamespace (namespace_name);

            // 抽象クラスフラグ設定
            builder.isAbstract = PlantUMLUtility.CheckContainsWords (words, "abstract");

            // 内容チェック
            if (lines[index].IndexOf ("{") < 0) {
                return new IContentBuilder[] { builder };
            }

            index++;

            // 定義終了まで内容をパース
            var abstract_regex = new Regex(@"_*abstract _*");
            while (lines[index].IndexOf ("}") < 0) {
                var member = new MemberInfo ();

                member.name = PlantUMLUtility.ReplaceAccessModifiers (lines[index]).TrimStart ();
                member.isAbstract = abstract_regex.IsMatch (lines [index]);

                if (!string.IsNullOrEmpty (member.name)) {
                    builder.AddMemberInfo (member);
                }

                index++;
            }

            return new IContentBuilder[] { builder };
        }
    }
}

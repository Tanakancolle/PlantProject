
namespace UML
{

    /// <summary>
    /// インターフェースパーサー
    /// </summary>
    public class InterfaceParser : IContentParser
    {
        public IContentBuilder[] Parse(string[] lines, ref int index, string namespace_name = "")
        {
            var words = PlantUMLUtility.SplitSpace (lines[index]);

            // コンテンツ名インデックス取得
            int name_index = PlantUMLUtility.GetContentNameIndexFromWords (words, "interface");
            if (name_index == -1) {
                return null;
            }
            
            var builder = new InterfaceBuilder ();

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
                if (string.IsNullOrEmpty (lines[index].Trim())) {
                    index++;
                    continue;
                }

                var member = new MemberInfo ();

                member.name = string.Format ("public {0}", lines[index].TrimStart ());
                member.isAbstract = true;

                builder.AddMemberInfo (member);    

                index++;
            }

            return new IContentBuilder[] { builder };
        }
    }
}

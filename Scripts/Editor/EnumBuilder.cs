using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace UML
{
    /// <summary>
    /// 列挙型ビルダー
    /// </summary>
    public class EnumBuilder : IContentBuilder
    {
        /// <summary>
        /// 列挙型名
        /// </summary>
        private string enumName;

        /// <summary>
        /// ネームスペース名
        /// </summary>
        private string namespaceName;

        /// <summary>
        /// メンバ配列
        /// </summary>
        private List<string> memberList = new List<string>();

        public void AddInheritance(IContentBuilder builder)
        {
            // 何もしない
        }

        public List<string> GetAbstractMember ()
        {
            return null;
        }

        public string GetName()
        {
            return enumName;
        }

        public void SetName(string name)
        {
            enumName = name;
        }

        public void SetNamespace(string name)
        {
            namespaceName = name;
        }
        
        public StringBuilder BuildScriptText(PlantUMLConvertOption option)
        {
            var builder = new StringBuilder ();
            var tab = string.Empty;
            int tab_num = 0;

            // ネームスペース開始チェック
            if (!option.isNonCreateNamespace && !string.IsNullOrEmpty (namespaceName)) {
                builder.AppendLine (string.Format ("namespace {0}", namespaceName));
                builder.AppendLine ("{");
                tab_num++;
            }

            // 列挙型定義開始
            tab = StringBuilderHelper.SetTab (tab_num);
            builder.AppendLine (tab + string.Format ("public enum {0}", enumName));
            builder.AppendLine (tab + "{");
            tab_num++;
            {
                tab = StringBuilderHelper.SetTab (tab_num);

                foreach (var name in GetDeclarationMemberNames ()) {
                    builder.AppendLine (tab + name);
                }
            }
            tab_num--;
            tab = StringBuilderHelper.SetTab (tab_num);
            builder.AppendLine (tab + "}");

            // ネームスペース終了チェック
            if (!option.isNonCreateNamespace && !string.IsNullOrEmpty (namespaceName)) {
                builder.AppendLine ("}");
            }

            return builder;
        }

        public void AddMember(string member)
        {
            memberList.Add (member);
        }

        public string[] GetAbstractMembers ()
        {
            return null;
        }

        /// <summary>
        /// 宣言するメンバ名
        /// </summary>
        private string[] GetDeclarationMemberNames()
        {
            return memberList.Select (member => member.IndexOf (",") >= 0 ? member : member + ",").ToArray ();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UML
{

    /// <summary>
    /// インターフェース情報
    /// </summary>
    public class InterfaceInfo : ContentInfoBase
    {
        public override StringBuilder BuildScriptText(PlantUMLConvertOption option)
        {
            var builder = new StringBuilder ();
            var using_list = new HashSet<string> ();

            // 改行
            builder.AppendLine ();

            var tab = string.Empty;
            int tab_num = 0;

            // ネームスペース開始チェック
            if (!option.isNonCreateNamespace && !string.IsNullOrEmpty (namespaceName)) {
                builder.AppendLine (string.Format ("namespace {0}", namespaceName));
                builder.AppendLine ("{");
                tab_num++;
            }

            // インターフェース定義開始
            tab = StringBuilderHelper.SetTab (tab_num);
            builder.AppendLine (tab + GetDeclarationName ());
            builder.AppendLine (tab + "{");
            tab_num++;
            {
                // メンバ宣言処理
                if (!option.isNonCreateMember) {
                    tab = StringBuilderHelper.SetTab (tab_num);

                    // 変数宣言
                    foreach (var name in GetDeclarationMethodNames ()) {
                        // メソッド宣言
                        builder.AppendLine (tab + name);                   

                        // 改行
                        builder.AppendLine ();

                        // usingリスト追加
                        foreach (var type_name in PlantUMLUtility.GetTypeNameFromDeclarationName (name)) {
                            var type = PlantUMLUtility.GetTypeFromTypeName (type_name);
                            if (type == null || string.IsNullOrEmpty (type.Namespace)) {
                                continue;
                            }

                            using_list.Add (type.Namespace);
                        }
                    }
                }
            }
            tab_num--;
            tab = StringBuilderHelper.SetTab (tab_num);
            builder.AppendLine (tab + "}");

            // ネームスペース終了チェック
            if (!option.isNonCreateNamespace && !string.IsNullOrEmpty (namespaceName)) {
                builder.AppendLine ("}");
            }

            if (option.declarationUsings != null) {
                foreach (var using_name in option.declarationUsings) {
                    using_list.Add (using_name);
                }
            }

            StringBuilderHelper.EditUsings (builder, using_list.ToArray ());

            return builder;
        }

        /// <summary>
        /// 宣言する名前
        /// </summary>
        private string GetDeclarationName()
        {
            return string.Format ("public interface {0}", contentName);
        }

        /// <summary>
        /// 宣言するメソッド名
        /// </summary>
        /// <returns>The declaration method names.</returns>
        private string[] GetDeclarationMethodNames()
        {
            var infos = GetDeclarationMemberInfos ();

            var names = infos.Select (x => x.name.Replace ("public", string.Empty).TrimStart ());

            return names.Select (x => x + (x.IndexOf (";") < 0 ? ";" : string.Empty)).ToArray ();
        }
    }
}
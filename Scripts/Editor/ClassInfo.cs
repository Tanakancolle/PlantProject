using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace UML
{

    /// <summary>
    /// クラス情報
    /// </summary>
    public class ClassInfo : ContentInfoBase
    {

        /// <summary>
        /// 抽象クラスフラグ
        /// </summary>
        public bool isAbstract = false;

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

            // クラス定義開始         
            tab = StringBuilderSupporter.SetTab (tab_num);
            builder.AppendLine (tab + GetDeclarationName ());
            builder.AppendLine (tab + "{");
            tab_num++;
            {
                // メンバ宣言処理
                if (!option.isNonCreateMember) {
                    tab = StringBuilderSupporter.SetTab (tab_num);

                    // 変数宣言
                    foreach (var name in GetDeclarationValueNames ()) {
                        // メンバ宣言
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

                    // 関数宣言
                    foreach (var name in GetDeclarationMethodNames ()) {
                        // メンバ宣言
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
            tab = StringBuilderSupporter.SetTab (tab_num);
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

            StringBuilderSupporter.EditUsings (builder, using_list.ToArray ());

            return builder;
        }

        /// <summary>
        /// 宣言する名前
        /// </summary>
        private string GetDeclarationName()
        {
            var declaration = string.Empty;

            // 抽象クラスチェック
            if (isAbstract) {
                declaration = string.Format ("public abstract class {0}", contentName);
            } else {
                declaration = string.Format ("public class {0}", contentName);
            }

            // 継承チェック
            if (inheritanceList != null && inheritanceList.Count > 0) {
                StringBuilder builder = new StringBuilder ();

                // 継承を再現
                foreach (var inheritance in inheritanceList.Select (x => x.GetName ())) {
                    if (builder.Length > 0) {
                        builder.Append (", ");
                    }

                    builder.Append (inheritance);
                }

                declaration = string.Format ("{0} : {1}", declaration, builder.ToString ());
            }

            return declaration;
        }

        /// <summary>
        /// 宣言する数値名
        /// </summary>
        private string[] GetDeclarationValueNames()
        {
            var infos = GetDeclarationMemberInfos ();

            // 関数チェック用
            var method_regex = new Regex (@"\(*\)");

            return infos.Where (x => !method_regex.IsMatch (x.name)).Select (x => x.name + (x.name.IndexOf (";") < 0 ? ";" : string.Empty)).ToArray ();
        }

        /// <summary>
        /// 宣言する関数名
        /// </summary>
        /// <returns>The declaration method names.</returns>
        private string[] GetDeclarationMethodNames()
        {
            // 関数チェック用
            var method_regex = new Regex (@"\(*\)");

            var list = new List<string> ();

            // 継承関数名
            foreach (var info in inheritanceList) {
                list.AddRange (info.GetAbstractMemberInfos ().
                    Where (x => method_regex.IsMatch (x.name)).
                    Select (x => x.name.Replace (";", string.Empty).Replace ("abstract", "override") + " {}")
                );
            }

            // メンバ関数名
            list.AddRange (memberList.Where (x => method_regex.IsMatch (x.name)).Select (x => {
                if (x.isAbstract) {
                    return x.name.IndexOf (";") < 0 ? x.name + ";" : x.name;
                }

                return x.name.Replace (";", string.Empty) + " {}";
            }));

            return list.ToArray ();
        }
    }
}
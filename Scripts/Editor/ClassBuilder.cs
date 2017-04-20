using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;

namespace UML
{

    /// <summary>
    /// クラス情報
    /// </summary>
    public class ClassBuilder : ContainerBuilderBase
    {
        /// <summary>
        /// 抽象クラスフラグ
        /// </summary>
        public bool isAbstract = false;

        /// <summary>
        /// 宣言する名前
        /// </summary>
        protected override string GetDeclarationName()
        {
            var declaration = string.Empty;

            // 抽象クラスチェック
            if (isAbstract) {
                declaration = string.Format ("public abstract class {0}", containerName);
            } else {
                declaration = string.Format ("public class {0}", containerName);
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
        protected override string[] GetDeclarationValueNames()
        {
            var infos = GetDeclarationMembers ();

            // 関数チェック用
            var method_regex = new Regex (@"\(*\)");

            return infos.Where (x => !method_regex.IsMatch (x)).Select (x => x + (x.IndexOf (";") < 0 ? ";" : string.Empty)).ToArray ();
        }

        /// <summary>
        /// 宣言する関数名
        /// </summary>
        /// <returns>The declaration method names.</returns>
        protected override string[] GetDeclarationMethodNames()
        {
            // 関数チェック用
            var method_regex = new Regex (@"\(*\)");

            var list = new List<string> ();

            // 継承関数名
            foreach (var info in inheritanceList) {
                list.AddRange (info.GetAbstractMembers ().
                    Where (x => method_regex.IsMatch (x)).
                    Select (x => x.Replace (";", string.Empty).Replace ("abstract", "override"))
                );
            }

            // メンバ関数名
            list.AddRange (memberList.Where (x => method_regex.IsMatch (x.name)).Select (x => {
                if (x.isAbstract) {
                    return x.name.IndexOf (";") < 0 ? x.name + ";" : x.name;
                }

                return x.name.Replace (";", string.Empty);
            }));

            // 返り値記述
            var abstract_regex = new Regex("_*abstract _*");
            for (int i = 0; i < list.Count; ++i) {
                if (abstract_regex.IsMatch (list [i])) {
                    continue;
                }

                var type_name = PlantUMLUtility.GetReturnTypeNameFromDeclarationName (list [i]);

                if (string.IsNullOrEmpty (type_name) || type_name == "void") {
                    list[i] += " {}";
                    continue;
                }

                var type = PlantUMLUtility.GetTypeFromTypeName (type_name);

                string return_name = null;
                if (type == null) {
                    return_name = "null";
                } else {
                    if (type.IsValueType) {
                        return_name = string.Format ("({0})0", type_name);
                    } else {
                        return_name = "null";
                    }
                }

                list[i] += " { return " + return_name + "; }"; 

            }

            return list.ToArray ();
        }
    }
}
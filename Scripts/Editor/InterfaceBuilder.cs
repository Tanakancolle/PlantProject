using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UML
{

    /// <summary>
    /// インターフェース情報
    /// </summary>
    public class InterfaceBuilder : ContainerBuilderBase
    {
        /// <summary>
        /// 宣言する名前
        /// </summary>
        protected  override string GetDeclarationName()
        {
            var declaration = string.Format ("public interface {0}", containerName);

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
        /// 宣言する変数名
        /// </summary>
        /// <returns></returns>
        protected override string[] GetDeclarationValueNames()
        {
            return new string[] {};
        }

        /// <summary>
        /// 宣言するメソッド名
        /// </summary>
        /// <returns>The declaration method names.</returns>
        protected override string[] GetDeclarationMethodNames()
        {                
            var names = memberList.Select (x => x.name.Replace ("public", string.Empty).TrimStart ());

            return names.Select (x => x + (x.IndexOf (";") < 0 ? ";" : string.Empty)).ToArray ();
        }
    }
}
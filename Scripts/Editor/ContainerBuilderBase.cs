using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UML
{
    /// <summary>
    /// コンテナビルダーベース
    /// </summary>
    public abstract class ContainerBuilderBase : IContentBuilder
    {
        /// <summary>
        /// コンテナ名
        /// </summary>
        protected string containerName;

        /// <summary>
        /// ネームスペース名
        /// </summary>
        protected string namespaceName;

        /// <summary>
        /// メンバーリスト
        /// </summary>
        protected List<MemberInfo> memberList = new List<MemberInfo> ();

        /// <summary>
        /// 継承リスト
        /// </summary>
        protected List<IContentBuilder> inheritanceList = new List<IContentBuilder> ();

        /// <summary>
        /// 継承追加
        /// </summary>
        /// <param name="builder"></param>
        public void AddInheritance (IContentBuilder builder)
        {
            inheritanceList.Add (builder);
        }

        /// <summary>
        /// スクリプトテキストビルド
        /// </summary>
        /// <returns>The script text.</returns>
        /// <param name="option">Option.</param>
        public virtual StringBuilder BuildScriptText(PlantUMLConvertOption option)
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
                if (!option.isNonCreateMember)
                {
                    EditValueMember(builder, tab_num, ref using_list);
                    EditMethodMember(builder, tab_num, ref using_list);
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
        /// <returns></returns>
        protected abstract string GetDeclarationName();

        /// <summary>
        /// 変数メンバ記述
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="tab_num"></param>
        /// <param name="using_list"></param>
        protected virtual void EditValueMember(StringBuilder builder, int tab_num, ref HashSet<string> using_list)
        {
            var tab = StringBuilderHelper.SetTab (tab_num);

            // 変数宣言
            foreach (var name in GetDeclarationValueNames ()) {
                // メンバ宣言
                builder.AppendLine (tab + name);

                // 改行
                builder.AppendLine ();

                // usingリスト追加
                foreach (var type_name in PlantUMLUtility.GetTypeNamesFromDeclarationName (name)) {
                    var type = PlantUMLUtility.GetTypeFromTypeName (type_name);
                    if (type == null || string.IsNullOrEmpty (type.Namespace)) {
                        continue;
                    }

                    using_list.Add (type.Namespace);
                }
            }
        }

        /// <summary>
        /// 関数メンバ記述
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="tab_num"></param>
        /// <param name="using_list"></param>
        protected virtual void EditMethodMember(StringBuilder builder, int tab_num, ref HashSet<string> using_list)
        {
            var tab = StringBuilderHelper.SetTab (tab_num);

            // 関数宣言
            foreach (var name in GetDeclarationMethodNames ()) {
                // メンバ宣言
                builder.AppendLine (tab + name);

                // 改行
                builder.AppendLine ();

                // usingリスト追加
                foreach (var type_name in PlantUMLUtility.GetTypeNamesFromDeclarationName (name)) {
                    var type = PlantUMLUtility.GetTypeFromTypeName (type_name);
                    if (type == null || string.IsNullOrEmpty (type.Namespace)) {
                        continue;
                    }

                    using_list.Add (type.Namespace);
                }
            }
        }

        /// <summary>
        /// 宣言する変数名配列取得
        /// </summary>
        /// <returns></returns>
        protected abstract string[] GetDeclarationValueNames();

        /// <summary>
        /// 宣言する関数名配列取得
        /// </summary>
        /// <returns></returns>
        protected abstract string[] GetDeclarationMethodNames();

        /// <summary>
        /// 抽象メンバ取得
        /// </summary>
        /// <returns>The abstract members.</returns>
        public virtual string[] GetAbstractMembers ()
        {
            var list = new List<string> ();

            foreach (var info in inheritanceList) {
                list.AddRange (info.GetAbstractMembers ());
            }

            list.AddRange (memberList.Where (member => member.isAbstract).Select (member => member.name));

            return list.ToArray ();
        }

        /// <summary>
        /// コンテナ名設定
        /// </summary>
        public virtual void SetName(string name)
        {
            containerName = name;
        }

        /// <summary>
        /// ネームスペース設定
        /// </summary>
        public virtual void SetNamespace(string namespace_name)
        {
            namespaceName = namespace_name;
        }

        /// <summary>
        /// コンテンツ名取得
        /// </summary>
        public virtual string GetName()
        {
            return containerName;
        }

        /// <summary>
        /// 宣言するメンバ名取得
        /// </summary>
        protected virtual List<string> GetDeclarationMembers()
        {
            var list = new List<string> ();
            foreach (var info in inheritanceList) {
                list.AddRange (info.GetAbstractMembers ());
            }

            list.AddRange (memberList.Select (x => x.name));

            return list;
        }

        /// <summary>
        /// メンバ情報追加
        /// </summary>
        public virtual void AddMemberInfo(MemberInfo info)
        {
            memberList.Add (info);
        }

        /// <summary>
        /// 継承情報追加
        /// </summary>
        public virtual void AddInhritanceInfo(ContainerBuilderBase info)
        {
            inheritanceList.Add (info);
        }
    }

    /// <summary>
    /// メンバー情報
    /// </summary>
    public class MemberInfo
    {
        public string name;
        public bool isAbstract;
    }
}
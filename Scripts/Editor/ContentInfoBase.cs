using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UML
{

    /// <summary>
    /// コンテンツ情報ベース
    /// </summary>
    public abstract class ContentInfoBase
    {
        /// <summary>
        /// コンテンツ名
        /// </summary>
        protected string contentName;

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
        protected List<ContentInfoBase> inheritanceList = new List<ContentInfoBase> ();

        /// <summary>
        /// コンテンツ名設定
        /// </summary>
        public virtual void SetName(string name)
        {
            contentName = name;
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
            return contentName;
        }

        /// <summary>
        /// スクリプトテキスト作成
        /// </summary>
        public abstract StringBuilder BuildScriptText(PlantUMLConvertOption option);

        /// <summary>
        /// 宣言するメンバ名取得
        /// </summary>
        protected virtual List<MemberInfo> GetDeclarationMemberInfos()
        {
            var list = new List<MemberInfo> ();
            foreach (var info in inheritanceList) {
                list.AddRange (info.GetAbstractMemberInfos ());
            }

            list.AddRange (memberList);

            return list;
        }

        /// <summary>
        /// 抽象メンバー名取得
        /// </summary>
        public virtual List<MemberInfo> GetAbstractMemberInfos()
        {
            var list = new List<MemberInfo> ();

            foreach (var info in inheritanceList) {
                list.AddRange (info.GetAbstractMemberInfos ());
            }

            list.AddRange (memberList.Where (member => member.isAbstract));

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
        public virtual void AddInhritanceInfo(ContentInfoBase info)
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
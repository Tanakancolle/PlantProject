using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// コンテンツ情報ベース
/// </summary>
public abstract class ContentInfoBase {
    /// <summary>
    /// コンテンツ名
    /// </summary>
    public string contentName;

    /// <summary>
    /// ネームスペース名
    /// </summary>
    public string namespaceName;

    /// <summary>
    /// メンバーリスト
    /// </summary>
    public List<MemberInfo> memberList = new List<MemberInfo>();

    /// <summary>
    /// 継承リスト
    /// </summary>
    public List<ContentInfoBase> inheritanceList = new List<ContentInfoBase>();

    /// <summary>
    /// コンテンツ名取得
    /// </summary>
    public virtual string GetName()
    {
        return contentName;
    }

    /// <summary>
    /// 宣言するコンテンツ名取得
    /// </summary>
    public abstract string GetDeclarationName ();

    /// <summary>
    /// 宣言するメンバ名取得
    /// </summary>
    public virtual List<MemberInfo> GetDeclarationMemberInfos() {
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
    public virtual MemberInfo[] GetAbstractMemberInfos()
    {
        if (memberList == null) {
            return null;
        }

        return memberList.Where (member => member.isAbstract).ToArray ();
    }

    /// <summary>
    /// 継承情報追加
    /// </summary>
    public virtual void AddInhritanceInfo(ContentInfoBase info)
    {
        inheritanceList.Add(info);
    }
}

/// <summary>
/// メンバー情報
/// </summary>
public class MemberInfo {
    public string name;
    public bool isAbstract;
}
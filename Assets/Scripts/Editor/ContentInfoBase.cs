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
    public List<MenberInfo> menberList = new List<MenberInfo>();

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
    /// 抽象メンバー名取得
    /// </summary>
    public virtual string[] GetAbstractMemberNames()
    {
        if (menberList == null) {
            return null;
        }

        return menberList.Where (member => member.isAbstract).Select (menber => menber.name).ToArray ();
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
public class MenberInfo {
    public string name;
    public bool isAbstract;
}
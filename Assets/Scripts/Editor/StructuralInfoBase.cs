using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 構造情報ベース
/// </summary>
public abstract class StructuralInfoBase {
    /// <summary>
    /// 構造名
    /// </summary>
    public string structuralName;

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
    public List<StructuralInfoBase> inheritanceList = new List<StructuralInfoBase>();

    /// <summary>
    /// 構造体名取得
    /// </summary>
    /// <returns>The name.</returns>
    public virtual string GetName()
    {
        return structuralName;
    }

    /// <summary>
    /// 宣言する構造体名取得
    /// </summary>
    public abstract string GetDeclarationName ();

    /// <summary>
    /// 継承メンバー名取得
    /// </summary>
    public virtual string[] GetInheritanceMemberNames()
    {
        if (menberList == null) {
            return null;
        }

        return menberList.Where (member => member.isAbstract).Select (menber => menber.name).ToArray ();
    }

    public virtual void AddInhritanceInfo(StructuralInfoBase info)
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// インターフェース情報
/// </summary>
public class InterfaceInfo : StructuralInfoBase {

//    public override int Parse (string[] lines, int index)
//    {
//        // 名前パース
//        SetName (lines [index]);
//
//        // 内容までインデックスをずらす
//        index++;
//        if (lines [index].IndexOf ("{") >= 0) {
//            index++;
//        }
//
//        // 定義終了まで内容をパース
//        menberList = new List<MenberInfo> ();
//        while (lines [index].IndexOf ("}") < 0) {
//            var menber = new MenberInfo ();
//
//            menber.name = string.Format ("public {0}", lines [index].TrimStart ());
//            menber.isAbstract = true;
//
//            menberList.Add (menber);
//
//            index++;
//        }
//
//        return index++;
//    }

//    public override void SetName (string structural)
//    {
//        strcturalName = structural.Replace ("interface", string.Empty).Replace ("{", string.Empty).Trim ();
//    }

    public override string GetDeclarationName ()
    {
        return string.Format ("public interface {0}", structuralName);
    }
}
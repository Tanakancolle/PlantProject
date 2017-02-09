using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

/// <summary>
/// クラス情報
/// </summary>
public class ClassInfo : ContentInfoBase
{

    /// <summary>
    /// 抽象クラスフラグ
    /// </summary>
    public bool isAbstract = false;

    public override string GetDeclarationName ()
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
            foreach (var inheritance in inheritanceList.Select(x => x.GetName())) {
                if (builder.Length > 0) {
                    builder.Append (", ");
                }

                builder.Append (inheritance);
            }

            declaration = string.Format ("{0} : {1}", declaration, builder.ToString ());
        }

        return declaration;
    }

    public override string[] GetDeclarationValueNames ()
    {
        var infos = GetDeclarationMemberInfos ();

        // 関数チェック用
        var method_regex = new Regex (@"\(*\)");

        return infos.Where (x => !method_regex.IsMatch (x.name)).Select (x => x.name + (x.name.IndexOf (";") < 0 ? ";" : string.Empty)).ToArray ();
    }

    public override string[] GetDeclarationMethodNames ()
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
        list.AddRange (memberList.Where(x=>method_regex.IsMatch(x.name)).Select (x => { 
            if( x.isAbstract ) {
                return x.name.IndexOf(";") < 0 ? x.name + ";" : x.name;
            }

            return x.name.Replace(";",string.Empty) + " {}";
        }));

        return list.ToArray ();
    }
}
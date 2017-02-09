using System.Linq;

/// <summary>
/// インターフェース情報
/// </summary>
public class InterfaceInfo : ContentInfoBase {
    public override string GetDeclarationName ()
    {
        return string.Format ("public interface {0}", contentName);
    }
   
    public override string[] GetDeclarationValueNames ()
    {
        // インターフェースにメンバ変数はない
        return new string[]{ };
    }

    public override string[] GetDeclarationMethodNames ()
    {
        var infos = GetDeclarationMemberInfos ();

        var names = infos.Select (x => x.name.Replace ("public", string.Empty).TrimStart ());

        return names.Select (x => x + (x.IndexOf (";") < 0 ? ";" : string.Empty)).ToArray ();
    }
}
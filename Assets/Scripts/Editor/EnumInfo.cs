using System.Linq;


/// <summary>
/// 列挙型情報
/// </summary>
public class EnumInfo : ContentInfoBase {
    public override string GetDeclarationName ()
    {
        return string.Format ("public enum {0}", contentName);
    }

    public override string[] GetDeclarationValueNames ()
    {
        // メンバ変数として返す
        // TODO : 変数、メソッドを一緒にしたほうがいいかも
        return memberList.Select (x => x.name.IndexOf (",") >= 0 ? x.name : x.name + ",").ToArray();
    }

    public override string[] GetDeclarationMethodNames ()
    {
        // メソッドなし
        return new string[] {};
    }
}

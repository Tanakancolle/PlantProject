using System.Linq;
using System.Text;


/// <summary>
/// 列挙型情報
/// </summary>
public class EnumInfo : ContentInfoBase
{

    public override StringBuilder BuildScriptText(PlantUMLConvertOption option)
    {
        var builder = new StringBuilder ();

        // 列挙型定義開始
        builder.AppendLine (GetDeclarationName ());
        builder.AppendLine ("{");
        {
            var tab = StringBuilderSupporter.SetTab (1);

            foreach (var name in GetDeclarationMemberNames ()) {
                builder.AppendLine (tab + name);
            }
        }
        builder.AppendLine ("}");

        return builder;
    }

    /// <summary>
    /// 宣言する名前
    /// </summary>
    private string GetDeclarationName()
    {
        return string.Format ("public enum {0}", contentName);
    }

    /// <summary>
    /// 宣言するメンバ名
    /// </summary>
    private string[] GetDeclarationMemberNames()
    {
        return memberList.Select (x => x.name.IndexOf (",") >= 0 ? x.name : x.name + ",").ToArray ();
    }
}

using System.Linq;
using System.Text;

/// <summary>
/// インターフェース情報
/// </summary>
public class InterfaceInfo : ContentInfoBase {
    public override System.Text.StringBuilder BuildScriptText ()
    {
        var builder = new StringBuilder ();

        // インターフェース定義開始
        builder.AppendLine (GetDeclarationName ());
        builder.AppendLine ("{");
        {
            var tab = StringBuilderSupporter.SetTab (1);

            // 変数宣言
            foreach (var name in GetDeclarationMethodNames()) {
                // メソッド宣言
                builder.AppendLine (tab + name);

                // 改行
                builder.AppendLine ();
            }
        }
        builder.AppendLine ("}");

        return builder;
    }

    /// <summary>
    /// 宣言する名前
    /// </summary>
    private string GetDeclarationName ()
    {
        return string.Format ("public interface {0}", contentName);
    }

    /// <summary>
    /// 宣言するメソッド名
    /// </summary>
    /// <returns>The declaration method names.</returns>
    private string[] GetDeclarationMethodNames ()
    {
        var infos = GetDeclarationMemberInfos ();

        var names = infos.Select (x => x.name.Replace ("public", string.Empty).TrimStart ());

        return names.Select (x => x + (x.IndexOf (";") < 0 ? ";" : string.Empty)).ToArray ();
    }
}
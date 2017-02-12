using System.Linq;
using System.Text;

/// <summary>
/// インターフェース情報
/// </summary>
public class InterfaceInfo : ContentInfoBase
{
    public override StringBuilder BuildScriptText(PlantUMLConvertOption option)
    {
        var builder = new StringBuilder ();

        // using宣言
        StringBuilderSupporter.EditUsings (builder, option.declarationUsings);

        // 改行
        builder.AppendLine ();

        var tab = string.Empty;
        int tab_num = 0;

        // ネームスペース開始チェック
        if (!option.isNonCreateNamespace && !string.IsNullOrEmpty (namespaceName)) {
            builder.AppendLine (string.Format ("namespace {0}", namespaceName));
            builder.AppendLine ("{");
            tab_num++;
        }

        // インターフェース定義開始            
        tab = StringBuilderSupporter.SetTab (tab_num);
        builder.AppendLine (tab + GetDeclarationName ());
        builder.AppendLine (tab + "{");
        tab_num++;
        {
            // メンバ宣言処理
            if (!option.isNonCreateMember) {
                tab = StringBuilderSupporter.SetTab (tab_num);

                // 変数宣言
                foreach (var name in GetDeclarationMethodNames ()) {
                    // メソッド宣言
                    builder.AppendLine (tab + name);

                    // 改行
                    builder.AppendLine ();
                }
            }
        }
        tab_num--;
        tab = StringBuilderSupporter.SetTab (tab_num);
        builder.AppendLine (tab + "}");

        // ネームスペース終了チェック
        if (!option.isNonCreateNamespace && !string.IsNullOrEmpty (namespaceName)) {
            builder.AppendLine ("}");
        }

        return builder;
    }

    /// <summary>
    /// 宣言する名前
    /// </summary>
    private string GetDeclarationName()
    {
        return string.Format ("public interface {0}", contentName);
    }

    /// <summary>
    /// 宣言するメソッド名
    /// </summary>
    /// <returns>The declaration method names.</returns>
    private string[] GetDeclarationMethodNames()
    {
        var infos = GetDeclarationMemberInfos ();

        var names = infos.Select (x => x.name.Replace ("public", string.Empty).TrimStart ());

        return names.Select (x => x + (x.IndexOf (";") < 0 ? ";" : string.Empty)).ToArray ();
    }
}
﻿using System.Linq;
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

        // インターフェース定義開始
        builder.AppendLine (GetDeclarationName ());
        builder.AppendLine ("{");
        {
            // メンバ宣言処理
            if (!option.isNonCreateMember) {
                var tab = StringBuilderSupporter.SetTab (1);

                // 変数宣言
                foreach (var name in GetDeclarationMethodNames ()) {
                    // メソッド宣言
                    builder.AppendLine (tab + name);

                    // 改行
                    builder.AppendLine ();
                }
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
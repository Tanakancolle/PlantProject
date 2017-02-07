using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using System.Text;

public class PlantUMLConverter {

    /// <summary>
    /// コンテンツ情報リスト
    /// </summary>
    private List<ContentInfoBase> structuralInfoList = new List<ContentInfoBase>();

    /// <summary>
    /// パーサー配列
    /// </summary>
    private IContentParser[] parsers = new IContentParser[] {
        new ClassParser(),
        new InterfaceParser()
    };

    /// <summary>
    /// 変換処理
    /// </summary>
    /// <param name="text">Text.</param>
    public void ConvertProcess(string text, string create_folder)
    {
        // １行毎に分割
        var lines = text.Replace ("\r\n", "\n").Split ('\n');

        // コンテンツパース処理
        for (int i = 0; i < lines.Length; ++i) {
            foreach (var parser in parsers) {
                var infos = parser.Parse (lines, ref i);
                if (infos == null) {
                    continue;
                }

                structuralInfoList.AddRange (infos);
            }
        }

        // 矢印パース
        ParseArrow (lines);
        
        // 継承矢印パターン
        var left_regex = new Regex(PlantUMLUtility.GetArrowExtensionLeftPattern());
        var right_regex = new Regex (PlantUMLUtility.GetArrowExtensionRightPattern ());

        for (int i = 0; i < lines.Length; ++i) {
            if (left_regex.IsMatch (lines [i])) {
                var structurals = left_regex.Split (lines [i]).Select (x => x.Trim ()).ToArray ();

                var base_structural = structuralInfoList.FirstOrDefault (x => x.GetName () == structurals [0]);

                var target_structural = structuralInfoList.FirstOrDefault (x => x.GetName () == structurals [1]);

                target_structural.AddInhritanceInfo (base_structural);
            } else if (right_regex.IsMatch (lines [i])) {
                var structurals = right_regex.Split (lines [i]).Select (x => x.Trim ()).ToArray ();

                if (structurals.Length < 2) {
                    Debug.LogError (structurals);
                }

                var base_structural = structuralInfoList.FirstOrDefault (x => x.GetName () == structurals [1]);

                var target_structural = structuralInfoList.FirstOrDefault (x => x.GetName () == structurals [0]);

                target_structural.AddInhritanceInfo (base_structural);
            }
        }

        var method_regex = new Regex (@"\(*\)");
        foreach( var info in structuralInfoList ) {
            StringBuilder builder = new StringBuilder ();
            string tab = string.Empty;

            // コンテンツ定義開始
            builder.AppendLine (tab + info.GetDeclarationName ());
            builder.AppendLine (tab + "{");
            {
                tab = StringBuilderSupporter.SetTab (1);
                foreach (var member in info.menberList) {
                    // メンバ宣言
                    builder.Append (tab + member.name);

                    // 関数処理
                    if (method_regex.IsMatch (member.name)) {
                        builder.AppendLine (" {}");
                    } else if (member.name.IndexOf (";") < 0) {
                        builder.AppendLine (";");
                    } else {
                        builder.AppendLine ();
                    }

                    // 改行
                    builder.AppendLine ();
                }
            }
            builder.AppendLine ("}");

            // スクリプト生成　※上書きは行わない
            StringBuilderSupporter.CreateScript (string.Format ("{0}/{1}.cs", create_folder.TrimEnd ('/'), info.GetName ()), builder.ToString (), false);
            StringBuilderSupporter.RefreshEditor ();
        }
    }

    /// <summary>
    /// 矢印パース
    /// </summary>
    private void ParseArrow(string[] lines )
    {
        // 矢印パターン読み込み
        Regex regex = new Regex (PlantUMLUtility.GetArrowPattern());

        foreach (var line in lines) {
            // 矢印チェック
            if (!regex.IsMatch (line)) {
                continue;
            }

            // クラス名取り出し
            var struct_names = regex.Split (line);
            foreach (var struct_name in struct_names) {
                var replace_name = struct_name.Trim ();

                // すでに登録されているか
                if (structuralInfoList.Any (x => x.GetName() == replace_name)) {
                    continue;
                }

                // 新コンテンツはクラスとする
                var info = new ClassInfo ();
                info.contentName = replace_name;

                // クラス登録
                structuralInfoList.Add (info);
            }
        }
    }
}

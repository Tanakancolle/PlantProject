using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using System.Text;

/// <summary>
/// PlantUMLコンバーター
/// </summary>
public class PlantUMLConverter {

    /// <summary>
    /// コンテンツ情報リスト
    /// </summary>
    private List<ContentInfoBase> contentInfoList = new List<ContentInfoBase>();

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

                contentInfoList.AddRange (infos);
            }
        }

        // 矢印パース処理
        ParseArrow (lines);

        // 継承パース処理
        ParseExtension(lines);

        // スクリプト生成処理
        CreateScripts(create_folder);
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
                if (contentInfoList.Any (x => x.GetName() == replace_name)) {
                    continue;
                }

                // 新コンテンツはクラスとする
                var info = new ClassInfo ();
                info.contentName = replace_name;

                // クラス登録
                contentInfoList.Add (info);
            }
        }
    }

    /// <summary>
    /// 継承パース
    /// </summary>
    private void ParseExtension(string[] lines) {
        // 継承矢印パターン
        var left_regex = new Regex(PlantUMLUtility.GetArrowExtensionLeftPattern());
        var right_regex = new Regex (PlantUMLUtility.GetArrowExtensionRightPattern ());

        // 矢印チェック
        for (int i = 0; i < lines.Length; ++i) {
            if (left_regex.IsMatch (lines [i])) {
                var contents = left_regex.Split (lines [i]).Select (x => x.Trim ()).ToArray ();

                var base_content = contentInfoList.FirstOrDefault (x => x.GetName () == contents [0]);
                var target_content = contentInfoList.FirstOrDefault (x => x.GetName () == contents [1]);

                // 継承情報追加
                target_content.AddInhritanceInfo (base_content);
            } else if (right_regex.IsMatch (lines [i])) {
                var contents = right_regex.Split (lines [i]).Select (x => x.Trim ()).ToArray ();

                var base_content = contentInfoList.FirstOrDefault (x => x.GetName () == contents [1]);
                var target_content = contentInfoList.FirstOrDefault (x => x.GetName () == contents [0]);

                // 継承情報追加
                target_content.AddInhritanceInfo (base_content);
            }
        }
    }

    /// <summary>
    /// スクリプト群生成
    /// </summary>
    private void CreateScripts(string create_folder) {
        var create_path = create_folder.TrimEnd ('/');
        var method_regex = new Regex (@"\(*\)");

        string tab = string.Empty;

        foreach( var info in contentInfoList ) {
            StringBuilder builder = new StringBuilder ();

            tab = string.Empty;

            // コンテンツ定義開始
            builder.AppendLine (tab + info.GetDeclarationName ());
            builder.AppendLine (tab + "{");
            {
                tab = StringBuilderSupporter.SetTab (1);
                
                // 変数と関数を分ける
                // TODO : 変数と関数は元から分けるように変更（定義パターンを作成）
                var variable_list = new List<MemberInfo> ();
                var method_list = new List<MemberInfo> ();

                foreach (var member in info.GetDeclarationMemberInfos ()) {
                    if (method_regex.IsMatch (member.name)) {
                        method_list.Add (member);
                    } else {
                        variable_list.Add (member);
                    }
                }

                // 変数宣言
                foreach (var variable in variable_list) {
                    // メンバ宣言
                    builder.AppendLine (tab + variable.name + (variable.name.IndexOf (";") < 0 ? ";" : string.Empty));

                    // 改行
                    builder.AppendLine ();
                }

                // 関数宣言
                foreach (var method in  method_list) {
                    // メンバ宣言
                    builder.AppendLine (tab + method.name + " {}");

                    // 改行
                    builder.AppendLine ();
                }
            }
            builder.AppendLine ("}");

            // スクリプト生成　※上書きは行わない
            if (!StringBuilderSupporter.CreateScript (string.Format ("{0}/{1}.cs", create_path, info.GetName ()), builder.ToString (), false)) {
                Debug.LogWarningFormat ("既に存在するため生成されませんでした：{0}.cs", info.GetName ());
            }
        }

        StringBuilderSupporter.RefreshEditor ();
    }
}

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
public class PlantUMLConverter
{

    /// <summary>
    /// コンテンツ情報リスト
    /// </summary>
    private List<ContentInfoBase> contentInfoList = new List<ContentInfoBase> ();

    /// <summary>
    /// パーサー配列
    /// </summary>
    private IContentParser[] parsers = new IContentParser[] {
        new ClassParser (),
        new InterfaceParser (),
        new EnumParser(),
    };

    /// <summary>
    /// 変換処理
    /// </summary>
    public void ConvertProcess (string text, string create_folder, PlantUMLConvertOption option)
    {
        // １行毎に分割
        var lines = text.Replace ("\r\n", "\n").Split ('\n');

        string namespace_name = string.Empty;

        for (int i = 0; i < lines.Length; ++i) {
            // ネームスペースパース処理　※未実装
            // TODO : 実装する！？
            namespace_name = ParseNamespace (lines, ref i);
            if (!string.IsNullOrEmpty (namespace_name)) {
                continue;
            }

            // コンテンツパース処理
            foreach (var parser in parsers) {
                var infos = parser.Parse (lines, ref i, namespace_name);
                if (infos == null) {
                    continue;
                }

                contentInfoList.AddRange (infos);
            }
        }

        // 矢印パース処理
        ParseArrow (lines, option.arrowPattern);

        // 継承パース処理
        ParseExtension (lines, option.arrowExtensionLeftPattern, option.arrowExtensionRightPattern);

        // スクリプト生成処理
        CreateScripts (create_folder);
    }

    /// <summary>
    /// ネームスペースパース
    /// </summary>
    private string ParseNamespace (string[] lines, ref int index)
    {
        var words = PlantUMLUtility.SplitSpace (lines [index].TrimStart ());

        // ネームスペースチェック
        if (!words [0].Contains ("namespace")) {
            return string.Empty;
        }

        return words [1];
    }

    /// <summary>
    /// 矢印パース
    /// </summary>
    private void ParseArrow (string[] lines, string pattern)
    {
        // 矢印パターン読み込み
        var regex = new Regex (pattern);

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
                if (contentInfoList.Any (x => x.GetName () == replace_name)) {
                    continue;
                }

                // 新コンテンツはクラスとする
                var info = new ClassInfo ();
                info.SetName (replace_name);

                // クラス登録
                contentInfoList.Add (info);
            }
        }
    }

    /// <summary>
    /// 継承パース
    /// </summary>
    private void ParseExtension (string[] lines, string left_pattern, string right_pattern)
    {
        // 継承矢印パターン
        var left_regex = new Regex (left_pattern);
        var right_regex = new Regex (right_pattern);

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
    private void CreateScripts (string create_folder)
    {
        var create_path = create_folder.TrimEnd ('/');

        foreach (var info in contentInfoList) {
            // スクリプトテキスト取得
            var builder = info.BuildScriptText ();

            // スクリプト生成　※上書きは行わない
            if (!StringBuilderSupporter.CreateScript (string.Format ("{0}/{1}.cs", create_path, info.GetName ()), builder.ToString (), false)) {
                Debug.LogWarningFormat ("既に存在するため生成されませんでした：{0}.cs", info.GetName ());
            }
        }

        StringBuilderSupporter.RefreshEditor ();
    }
}

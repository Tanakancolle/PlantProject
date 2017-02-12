using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

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
    public void ConvertProcess(string text, PlantUMLConvertOption option, bool is_check )
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
        ParseArrow (lines, PlantUMLUtility.ReplaceDirPattern (option.arrowPattern));

        // 継承パース処理
        ParseExtension (lines, PlantUMLUtility.ReplaceDirPattern (option.arrowExtensionLeftPattern), PlantUMLUtility.ReplaceDirPattern (option.arrowExtensionRightPattern));

        // スクリプト生成処理
        CreateScripts (option, is_check);
    }

    /// <summary>
    /// ネームスペースパース
    /// </summary>
    private string ParseNamespace(string[] lines, ref int index)
    {
        var words = PlantUMLUtility.SplitSpace (lines[index].TrimStart ());

        // ネームスペースチェック
        if (!words[0].Contains ("namespace")) {
            return string.Empty;
        }

        return words[1];
    }

    /// <summary>
    /// 矢印パース
    /// </summary>
    private void ParseArrow(string[] lines, string pattern)
    {
        // パターンが空だったら終了
        if (string.IsNullOrEmpty (pattern)) {
            return;
        }

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
    private void ParseExtension(string[] lines, string left_pattern, string right_pattern)
    {
        // 継承矢印パターン
        Regex left_regex = null;
        Regex right_regex = null;

        if (!string.IsNullOrEmpty (left_pattern)) {
            left_regex = new Regex (left_pattern);
        }

        if (!string.IsNullOrEmpty (right_pattern)) {
            right_regex = new Regex (right_pattern);
        }

        // 両方共パターンが空だったら終了
        if( left_regex == null && right_regex == null) {
            return;
        }

        // 矢印チェック
        for (int i = 0; i < lines.Length; ++i) {
            if (left_regex != null && left_regex.IsMatch (lines[i])) {
                var contents = left_regex.Split (lines[i]).Select (x => x.Trim ()).ToArray ();

                var base_content = contentInfoList.FirstOrDefault (x => x.GetName () == contents[0]);
                var target_content = contentInfoList.FirstOrDefault (x => x.GetName () == contents[1]);

                // 継承情報追加
                target_content.AddInhritanceInfo (base_content);
            } else if (right_regex != null && right_regex.IsMatch (lines[i])) {
                var contents = right_regex.Split (lines[i]).Select (x => x.Trim ()).ToArray ();

                var base_content = contentInfoList.FirstOrDefault (x => x.GetName () == contents[1]);
                var target_content = contentInfoList.FirstOrDefault (x => x.GetName () == contents[0]);

                // 継承情報追加
                target_content.AddInhritanceInfo (base_content);
            }
        }
    }

    /// <summary>
    /// スクリプト群生成
    /// </summary>
    private void CreateScripts(PlantUMLConvertOption option, bool is_check)
    {
        var create_path = option.createFolderPath.TrimEnd ('/');

        foreach (var info in contentInfoList) {
            // スクリプトテキスト取得
            var builder = info.BuildScriptText (option);

            // チェックのみか
            if(is_check) {
                Debug.LogFormat ("----{0}/{1}.cs----\n{2}----end----", create_path, info.GetName (), builder.ToString ());
                continue;
            }

            // スクリプト生成　※上書きは行わない
            if (StringBuilderSupporter.CreateScript (string.Format ("{0}/{1}.cs", create_path, info.GetName ()), builder.ToString (), false)) {
                Debug.LogFormat ("生成しました！：{0}.cs", info.GetName ());
            } else {
                Debug.LogWarningFormat ("既に存在するため生成されませんでした：{0}.cs", info.GetName ());
            }
        }

        StringBuilderSupporter.RefreshEditor ();
    }
}

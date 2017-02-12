using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System;

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
    /// ネームスペース処理中に行うデリゲート
    /// </summary>
    /// <param name="lines">文字配列</param>
    /// <param name="index">現在のインデックス</param>
    /// <param name="namespace_name">ネームスペース名</param>
    private delegate bool NamespaceCenterAction(string[] lines, ref int index, string namespace_name);

    /// <summary>
    /// ネームスペース処理中に行うデリゲート
    /// </summary>
    /// <param name="lines">文字配列</param>
    /// <param name="index">現在のインデックス</param>
    /// <param name="namespace_name">ネームスペース名</param>
    private delegate void NamespaceAfterAction(string[] lines, ref int index, string namespace_name);

    /// <summary>
    /// 矢印用正規表現
    /// </summary>
    private Regex arrowRegex;

    /// <summary>
    /// 変換処理
    /// </summary>
    public void ConvertProcess(string text, PlantUMLConvertOption option, bool is_check )
    {
        // １行毎に分割
        var lines = text.Replace ("\r\n", "\n").Split ('\n');

        // コンテンツパース処理
        ParseNamespaceProcess (lines, ParseContents, null);

        // 矢印パース処理
        if (!string.IsNullOrEmpty (option.arrowPattern)) {
            arrowRegex = new Regex (PlantUMLUtility.ReplaceDirPattern (option.arrowPattern));
            ParseNamespaceProcess (lines, null, ParseArrow);
        }                             

        // 継承パース処理
        ParseExtensionProcess (lines, PlantUMLUtility.ReplaceDirPattern (option.arrowExtensionLeftPattern), PlantUMLUtility.ReplaceDirPattern (option.arrowExtensionRightPattern));

        // スクリプト生成処理
        CreateScripts (option, is_check);
    }

    /// <summary>
    /// ネームスペースパース処理
    /// </summary>
    /// <param name="lines">チェック文字配列</param>
    /// <param name="center_action">開始チェックと終了チェックの間で行う処理</param>
    /// <param name="after_action">終了チェック後に行う処理</param>
    private void ParseNamespaceProcess(string[] lines, NamespaceCenterAction center_action, NamespaceAfterAction after_action)
    {   
        // ネームスペース関連
        var namespace_stack = new Stack<string> ();
        int scope_count = 0;
        var namespace_name = string.Empty;

        for (int i = 0; i < lines.Length; ++i) {
            // ネームスペースパース処理
            var parse_name = ParseNamespace (lines, ref i);
            if (!string.IsNullOrEmpty (parse_name)) {
                namespace_stack.Push (parse_name);
                namespace_name = BuildNamespace (namespace_stack);
                continue;
            }                                                 

            // 中央処理チェック
            if( center_action != null ) {
                if(center_action.Invoke (lines, ref i,namespace_name)) {
                    continue;
                }
            }

            // 管理外のスコープチェック
            if (lines[i].IndexOf ("{") >= 0) {
                scope_count++;
            }

            // ネームスペース終了チェック          
            if (lines[i].IndexOf ("}") >= 0) {
                if (scope_count > 0) {
                    scope_count--;
                } else {
                    namespace_stack.Pop ();
                    namespace_name = BuildNamespace (namespace_stack);
                }

                continue;
            }     

            // アフター処理チェック
            if (after_action != null) {
                after_action.Invoke (lines, ref i, namespace_name);
            }
        }
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

        return words[1].Replace ("{", string.Empty);
    }

    /// <summary>
    /// ネームスペース名ビルド
    /// </summary>              
    private string BuildNamespace(Stack<string> namespace_stack)
    {
        var builder = new StringBuilder ();
        for (int i = namespace_stack.Count - 1; i >= 0; --i) {
            if (builder.Length != 0) {
                builder.Append (".");
            }

            builder.Append (namespace_stack.ElementAt (i));
        }

        return builder.ToString ();
    } 

    /// <summary>
    /// ネームスペースとコンテンツ名に分割
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    private string[] SplitNamespaceAndContentName(string name)
    {
        var splits = new string[2];

        var end_index = name.LastIndexOf (".");

        if(end_index < 0) {
            splits[0] = string.Empty;
            splits[1] = name;

            return splits;
        }

        splits[0] = name.Substring (0, end_index);
        splits[1] = name.Substring (end_index + 1);

        return splits;
    }
                             
    /// <summary>
    /// コンテンツパース処理
    /// </summary>
    private bool ParseContents(string[] lines, ref int index, string namespace_name)
    {
        ContentInfoBase[] infos = null;
        foreach (var parser in parsers) {
            infos = parser.Parse (lines, ref index, namespace_name);
            if (infos != null) {
                break;
            }
        }

        if (infos != null) {
            contentInfoList.AddRange (infos);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 矢印パース
    /// </summary>
    private void ParseArrow(string[] lines, ref int index, string namespace_name)
    {
        // 矢印チェック
        if (!arrowRegex.IsMatch (lines[index])) {
            return;
        }

        // クラス名取り出し
        var struct_names = arrowRegex.Split (lines[index]);
        foreach (var struct_name in struct_names) {
            var splits = SplitNamespaceAndContentName (struct_name.Trim ());

            // すでに登録されているか
            if (contentInfoList.Any (x => x.GetName () == splits[1])) {
                continue;
            }

            // 新コンテンツはクラスとする
            var info = new ClassInfo ();
            info.SetNamespace (splits[0] == string.Empty ? namespace_name : splits[0]);
            info.SetName (splits[1]);

            // クラス登録
            contentInfoList.Add (info);
        }
    }

    /// <summary>
    /// 継承パース
    /// </summary>
    private void ParseExtensionProcess(string[] lines, string left_pattern, string right_pattern)
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

                var base_splits = SplitNamespaceAndContentName (contents[0]);
                var target_splits = SplitNamespaceAndContentName (contents[1]);

                var base_content = contentInfoList.FirstOrDefault (x => x.GetName () == base_splits[1]);
                var target_content = contentInfoList.FirstOrDefault (x => x.GetName () == target_splits[1]);

                // 継承情報追加
                target_content.AddInhritanceInfo (base_content);
            } else if (right_regex != null && right_regex.IsMatch (lines[i])) {
                var contents = right_regex.Split (lines[i]).Select (x => x.Trim ()).ToArray ();

                var base_splits = SplitNamespaceAndContentName (contents[1]);
                var target_splits = SplitNamespaceAndContentName (contents[0]);

                var base_content = contentInfoList.FirstOrDefault (x => x.GetName () == base_splits[1]);
                var target_content = contentInfoList.FirstOrDefault (x => x.GetName () == target_splits[1]);

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

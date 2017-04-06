using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace UML
{

    /// <summary>
    /// PlantUMLコンバーター
    /// </summary>
    public class PlantUMLConverter
    {
        public class LineInfo
        {
            public string line;

            public string namespaceName;
        }

        /// <summary>
        /// コンテンツ情報リスト
        /// </summary>
        private List<IContentBuilder> contentBuilderList = new List<IContentBuilder> ();

        /// <summary>
        /// パーサー配列
        /// </summary>
        private IContentParser[] parsers = new IContentParser[] {
            new ClassParser (),
            new InterfaceParser (),
            new EnumParser(),
        };                                                                                             

        /// <summary>
        /// 矢印用正規表現
        /// </summary>
        private Regex arrowRegex;

        /// <summary>
        /// 左継承矢印用正規表現
        /// </summary>
        private Regex arrowExtensionLeftRegex;

        /// <summary>
        /// 右継承矢印用正規表現
        /// </summary>
        private Regex arrowExtensionRightRegex;

        /// <summary>
        /// 矢印用ライン情報リスト
        /// </summary>
        private List<LineInfo> arrowLineInfoList;

        /// <summary>
        /// 左継承矢印用ライン情報リスト
        /// </summary>
        private List<LineInfo> arrowExtensionLeftLineInfoList;

        /// <summary>
        /// 右継承矢印用ライン情報リスト
        /// </summary>
        private List<LineInfo> arrowExtensionRightLineInfoList;

        /// <summary>
        /// 変換処理
        /// </summary>
        public void ConvertProcess(string text, PlantUMLConvertOption option, bool is_check)
        {
            // １行毎に分割
            var lines = text.Replace ("\r\n", "\n").Split ('\n');

            // 矢印系の初期化 
            {
                if (!string.IsNullOrEmpty (option.arrowPattern)) {
                    arrowLineInfoList = new List<LineInfo> ();
                    arrowRegex = new Regex (PlantUMLUtility.ReplaceDirPattern (option.arrowPattern));
                }

                if (!string.IsNullOrEmpty (option.arrowExtensionLeftPattern)) {
                    arrowExtensionLeftLineInfoList = new List<LineInfo> ();
                    arrowExtensionLeftRegex = new Regex (PlantUMLUtility.ReplaceDirPattern (option.arrowExtensionLeftPattern));
                }

                if (!string.IsNullOrEmpty (option.arrowExtensionRightPattern)) {
                    arrowExtensionRightLineInfoList = new List<LineInfo> ();
                    arrowExtensionRightRegex = new Regex (PlantUMLUtility.ReplaceDirPattern (option.arrowExtensionRightPattern));
                }
            }

            // パース処理
            ParseProcess (lines);

            // 矢印系のパース 
            {
                ParseArrow ();
                ParseArrowExtensionLeft ();
                ParseArrowExtensionRight ();
            }

            // スクリプト生成処理
            CreateScripts (option, is_check);
        }

        /// <summary>
        /// パース処理
        /// </summary>
        /// <param name="lines">チェック文字配列</param>
        private void ParseProcess(string[] lines)
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
                    namespace_name = BuildNamespaceText (namespace_stack);
                    continue;
                }

                // コンテンツパース
                if (ParseContents (lines, ref i, namespace_name)) {
                    continue;
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
                        namespace_name = BuildNamespaceText (namespace_stack);
                    }

                    continue;
                }

                // 矢印チェック
                {
                    CheckArrowLineAndAddList (lines[i], namespace_name);
                    CheckArrowExtensionLeftLineAndAddList (lines[i], namespace_name);
                    CheckArrowExtensionRightLineAndAddList (lines[i], namespace_name);
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
        private string BuildNamespaceText(Stack<string> namespace_stack)
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
        private string[] SplitNamespaceAndContentName(string name)
        {
            var splits = new string[2];

            var end_index = name.LastIndexOf (".");

            if (end_index < 0) {
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
            IContentBuilder[] infos = null;

            // パーサー毎にチェック
            foreach (var parser in parsers) {
                infos = parser.Parse (lines, ref index, namespace_name);

                // パースされたら終了
                if (infos != null) {
                    contentBuilderList.AddRange (infos);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 矢印行かチェックしてリストに追加
        /// </summary>                              
        private void CheckArrowLineAndAddList( string line, string namespace_name)
        {
            // 矢印チェック
            if (arrowRegex == null || !arrowRegex.IsMatch (line)) {
                return;
            }

            var info = new LineInfo ();
            info.line = line;
            info.namespaceName = namespace_name;

            arrowLineInfoList.Add (info);
        }

        /// <summary>
        /// 左継承矢印行かチェックしてリストに追加
        /// </summary>
        private void CheckArrowExtensionLeftLineAndAddList( string line, string namespace_name)
        {
            // 矢印チェック
            if (arrowExtensionLeftRegex == null || !arrowExtensionLeftRegex.IsMatch (line)) {
                return;
            }

            var info = new LineInfo ();
            info.line = line;
            info.namespaceName = namespace_name;

            arrowExtensionLeftLineInfoList.Add (info);
        }

        /// <summary>
        /// 右継承矢印行かチェックしてリストに追加
        /// </summary>                            
        private void CheckArrowExtensionRightLineAndAddList(string line, string namespace_name)
        {
            // 矢印チェック
            if (arrowExtensionRightRegex == null || !arrowExtensionRightRegex.IsMatch (line)) {
                return;
            }

            var info = new LineInfo ();
            info.line = line;
            info.namespaceName = namespace_name;

            arrowExtensionRightLineInfoList.Add (info);
        }

        /// <summary>
        /// 矢印パース
        /// </summary>
        private void ParseArrow()
        {   
            if( arrowLineInfoList == null) {
                return;
            }

            foreach(var line_info in arrowLineInfoList) {
                // クラス名取り出し
                var struct_names = arrowRegex.Split (line_info.line).Select (x => RemoveNotIncludedInContentName (x));
                foreach (var struct_name in struct_names) {
                    // ネームスペースと分割 ※「.」のパターン
                    var splits = SplitNamespaceAndContentName (struct_name);

                    // すでに登録されているか
                    if (contentBuilderList.Any (x => x.GetName () == splits[1])) {
                        continue;
                    }

                    // 新コンテンツはクラスとする
                    var builder = new ClassBuilder ();
                    builder.SetNamespace (splits[0] == string.Empty ? line_info.namespaceName : splits[0]);
                    builder.SetName (splits[1]);

                    // クラス登録
                    contentBuilderList.Add (builder);
                }

            }
        }      
        
        /// <summary>
        /// 左継承矢印のパース
        /// </summary>
        private void ParseArrowExtensionLeft()
        {
            if(arrowExtensionLeftLineInfoList == null) {
                return;
            }

            foreach( var info in arrowExtensionLeftLineInfoList) {              
                var contents = arrowExtensionLeftRegex.Split (info.line).Select (x => RemoveNotIncludedInContentName (x)).ToArray ();

                var base_splits = SplitNamespaceAndContentName (contents[0]);
                var target_splits = SplitNamespaceAndContentName (contents[1]);

                var base_content = contentBuilderList.FirstOrDefault (x => x.GetName () == base_splits[1]);
                var target_content = contentBuilderList.FirstOrDefault (x => x.GetName () == target_splits[1]);

                // 継承情報追加
                target_content.AddInheritance (base_content);
            }
        }    

        /// <summary>
        /// 右継承矢印のパース
        /// </summary>
        private void ParseArrowExtensionRight()
        {
            if(arrowExtensionRightLineInfoList == null) {
                return;
            }

            foreach( var info in arrowExtensionRightLineInfoList) { 
                var contents = arrowExtensionRightRegex.Split (info.line).Select (x => RemoveNotIncludedInContentName (x)).ToArray ();

                var base_splits = SplitNamespaceAndContentName (contents[1]);
                var target_splits = SplitNamespaceAndContentName (contents[0]);

                var base_content = contentBuilderList.FirstOrDefault (x => x.GetName () == base_splits[1]);
                var target_content = contentBuilderList.FirstOrDefault (x => x.GetName () == target_splits[1]);

                // 継承情報追加
                target_content.AddInheritance (base_content);

            }
        }

        /// <summary>
        /// コンテンツ名の不要文字削除
        /// </summary>                 
        private string RemoveNotIncludedInContentName(string line)
        {                       
            line = Regex.Replace (line, "\".*\"", string.Empty).Replace (" ", string.Empty);       

            var index = line.IndexOf (":");

            if (index < 0) {
                return line;
            }

            return line.Substring (0, index);
        }

        /// <summary>
        /// スクリプト群生成
        /// </summary>
        private void CreateScripts(PlantUMLConvertOption option, bool is_check)
        {
            var create_path = option.createFolderPath.TrimEnd ('/');

            foreach (var info in contentBuilderList) {
                // スクリプトテキスト取得
                var builder = info.BuildScriptText (option);

                // チェックのみか
                if (is_check) {
                    Debug.LogFormat ("----{0}/{1}.cs----\n{2}----end----", create_path, info.GetName (), builder.ToString ());
                    continue;
                }

                // スクリプト生成　※上書きは行わない
                if (StringBuilderHelper.CreateScript (string.Format ("{0}/{1}.cs", create_path, info.GetName ()), builder.ToString (), false)) {
                    Debug.LogFormat ("生成しました！：{0}.cs", info.GetName ());
                } else {
                    Debug.LogWarningFormat ("既に存在するため生成されませんでした：{0}.cs", info.GetName ());
                }
            }

            StringBuilderHelper.RefreshEditor ();
        }
    }
}

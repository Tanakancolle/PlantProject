using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;

namespace UML
{

    /// <summary>
    /// PlantUML系スクリプトの共有クラス
    /// </summary>
    public static class PlantUMLUtility
    {
        /// <summary>
        /// 方向パターン
        /// </summary>
        private const string dirPattern = @"(?:|right|left|up|down|r|l|u|d)";

        /// <summary>
        /// アクセス修飾子テーブル
        /// </summary>
        private static readonly Dictionary<string, string> accessModifiersTable = new Dictionary<string, string> () {
            {"+", "public "},
            {"-", "private " },
            {"#", "protected " }
        };

        /// <summary>
        /// 非型名
        /// </summary>
        private static readonly string[] nonTypeNames = new string[] {
            "public", "private", "protected",
            "virtual", "abstract", "override",
            "static", "readonly", "const"
        };

        /// <summary>
        /// 非引数型名
        /// </summary>
        private static readonly string[] nonArgumentTypeName = new string[] {
            "ref", "out"
        };

        /// <summary>
        /// 非検索アセンブリ名
        /// </summary>
        private static readonly string[] nonFindAssemblyName = new string[] {
            "SyntaxTree.*",
            "Mono.Cecil",
            "Boo.Lang"
        };

        /// <summary>
        /// アセンブリリスト
        /// </summary>
        private static List<Assembly> assemblyList;

        /// <summary>
        /// 方向パターン置き換え
        /// </summary>
        /// <returns>置き換え後文字列</returns>
        public static string ReplaceDirPattern(string text)
        {
            return text.Replace ("{dir}", PlantUMLUtility.dirPattern);
        }

        /// <summary>
        /// アクセス修飾子置き換え
        /// </summary>
        /// <returns>置き換え後文字列</returns>
        /// <param name="line">文字列</param>
        public static string ReplaceAccessModifiers(string line)
        {
            foreach (var replace in PlantUMLUtility.accessModifiersTable) {
                if (line.IndexOf (replace.Key) >= 0) {
                    return line.Replace (replace.Key, replace.Value);
                }
            }

            // アクセス修飾子がない場合はそのまま返す
            return line;
        }

        /// <summary>
        /// スペース毎に分割
        /// </summary>
        public static string[] SplitSpace(string line)
        {
            return line.Split (' ');
        }

        /// <summary>
        /// 同ワードチェック
        /// </summary>
        /// <param name="words">ワード配列</param>
        /// <param name="check_word">チェックワード</param>
        /// <returns><c>true</c>, ワード有り, <c>false</c> ワード無し.</returns>
        public static bool CheckContainsWords(string[] words, string check_word)
        {
            foreach (var word in words) {
                if (word.Equals (check_word)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// ワードのインデックス
        /// </summary>
        /// <param name="words">ワード配列</param>
        /// <param name="check_word">チェックワード</param>
        /// <returns>ワードのインデックス</returns>
        public static int IndexOfWords(string[] words, string check_word)
        {
            for (int i = 0; i < words.Length; ++i) {
                if (words[i].Equals (check_word)) {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// ワード配列からコンテンツ名のインデックス取得
        /// </summary>
        /// <param name="words">ワード配列</param>
        /// <param name="content">コンテンツワード</param>
        /// <returns>コンテンツ名のインデックス</returns>
        public static int GetContentNameIndexFromWords(string[] words, string content)
        {
            // コンテンツかチェック
            int content_index = PlantUMLUtility.IndexOfWords (words, content);
            if (content_index == -1) {
                return -1;
            }

            // asチェック
            int as_index = PlantUMLUtility.IndexOfWords (words, "as");
            if (as_index >= 0) {
                content_index = as_index;
            }

            for (int i = content_index + 1; i < words.Length; ++i) {
                if (!string.IsNullOrEmpty (words[i])) {
                    return i;
                }
            }

            // TODO : 詳細をだすように
            Debug.LogErrorFormat ("コンテンツ名の取得に失敗しました");

            return -1;
        }

        /// <summary>
        /// 宣言名からタイプ名取得
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetTypeNameFromDeclarationName(string name)
        {
            var words = PlantUMLUtility.SplitSpace (name);

            // 変数の宣言　＆　関数の返り値対応
            foreach (var word in words) {
                if (string.IsNullOrEmpty (word) || PlantUMLUtility.CheckContainsWords (PlantUMLUtility.nonTypeNames, word)) {
                    continue;
                }

                yield return word;               

                break;
            }

            // 関数の引数
            foreach (var type_name in GetTypeNameInRange ('(', ')', name)) {
                yield return type_name;
            }

            // ジェネリック対応
            foreach (var type_name in GetTypeNameInRange ('<', '>', name)) {
                yield return type_name;
            }
        }

        /// <summary>
        /// 範囲内のタイプ名取得
        /// </summary>                         
        private static IEnumerable<string> GetTypeNameInRange(char start, char end, string text)
        {
            int index = 0;
            while (true) {
                var start_index = text.IndexOf (start, index);
                if (start_index == -1) {
                    yield break;
                }

                var end_index = text.IndexOf (end, index);
                if (end_index == -1) {
                    yield break;
                }

                index = end_index;

                // 範囲取得
                start_index++;
                var argument_texts = text.Substring (start_index, end_index - start_index);

                // タイプ名取得
                foreach (var argument_text in argument_texts.Split (',')) {
                    foreach (var argument in PlantUMLUtility.SplitSpace (argument_text)) {
                        if (string.IsNullOrEmpty (argument) || PlantUMLUtility.CheckContainsWords (PlantUMLUtility.nonArgumentTypeName, argument)) {
                            continue;
                        }

                        yield return argument;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// タイプ名からタイプを取得
        /// </summary>
        public static Type GetTypeFromTypeName(string type_name) {
            // 検索アセンブリ取得
            if(assemblyList == null) {
                assemblyList = new List<Assembly> ();
                assemblyList.AddRange (AppDomain.CurrentDomain.GetAssemblies ().Where (x => !nonFindAssemblyName.Any (n => Regex.IsMatch (x.GetName ().Name, n))));
            }

            // ジェネリック対応処理
            int index = type_name.IndexOf ("<");
            if (index >= 0) {
                type_name = type_name.Remove (index) + "`1";
            }

            // タイプ検索
            foreach( var assembly in assemblyList){
                foreach (var type in assembly.GetTypes()) {
                    if (type.Name.Equals (type_name)) {
                        return type;
                    }
                }
            }

            return null;
        }

    }
}

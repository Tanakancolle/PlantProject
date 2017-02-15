using System.Collections.Generic;
using UnityEngine;

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
            for( int i = 0; i < words.Length; ++i) {
                if(words[i].Equals (check_word)) {
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
    }
}

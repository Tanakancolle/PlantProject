using UnityEngine;

namespace UML
{

    /// <summary>
    /// PlantUML変換オプション
    /// </summary>
    public class PlantUMLConvertOption : ScriptableObject
    {
        /// <summary>
        /// 生成フォルダパス
        /// </summary>
        public string createFolderPath = "Assets/";

        /// <summary>
        /// 矢印パターン
        /// </summary>
        public string arrowPattern = @"(?:<\|-{0,}{dir}-{1,}|-{1,}{dir}-{0,}\|>|<-{0,}{dir}-{1,}|-{1,}{dir}-{0,}>)";

        /// <summary>
        /// 拡張左矢印パターン
        /// </summary>
        public string arrowExtensionLeftPattern = @"(?:<\|-{0,}{dir}-{1,})";

        /// <summary>
        /// 拡張右矢印パターン
        /// </summary>
        public string arrowExtensionRightPattern = @"(?:-{1,}{dir}-{0,}\|>)";

        /// <summary>
        /// ネームスペースの非生成フラグ
        /// </summary>
        public bool isNonCreateNamespace = false;

        /// <summary>
        /// メンバの非生成フラグ
        /// </summary>
        public bool isNonCreateMember = false;

        /// <summary>
        /// 宣言Using
        /// </summary>
        public string[] declarationUsings;

        /// <summary>
        /// コピー
        /// </summary>
        public PlantUMLConvertOption Copy()
        {
            var option = ScriptableObject.CreateInstance<PlantUMLConvertOption> ();

            Copy (option);

            return option;
        }

        /// <summary>
        /// コピー
        /// </summary>
        /// <param name="option">書き込むオプション</param>
        public void Copy(PlantUMLConvertOption option)
        {
            option.createFolderPath = createFolderPath;
            option.arrowPattern = arrowPattern;
            option.arrowExtensionLeftPattern = arrowExtensionLeftPattern;
            option.arrowExtensionRightPattern = arrowExtensionRightPattern;
            option.isNonCreateNamespace = isNonCreateNamespace;
            option.isNonCreateMember = isNonCreateMember;

            if (declarationUsings != null || declarationUsings.Length != 0) {
                option.declarationUsings = new string[declarationUsings.Length];
                for (int i = 0; i < declarationUsings.Length; ++i) {
                    option.declarationUsings [i] = declarationUsings [i];
                }
            }
        }
    }
}

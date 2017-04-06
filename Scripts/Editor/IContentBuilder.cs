using UML;
using System.Text;
using System.Collections.Generic;

namespace UML
{
    /// <summary>
    /// コンテンツビルダーインターフェース
    /// </summary>
    public interface IContentBuilder
    {
        /// <summary>
        /// 継承追加
        /// </summary>
        /// <param name="builder">Builder.</param>
        void AddInheritance(IContentBuilder builder);

        /// <summary>
        /// 抽象メンバーリスト取得
        /// </summary>
        string[] GetAbstractMembers();

        /// <summary>
        /// 名前取得
        /// </summary>
        string GetName();

        /// <summary>
        /// スクリプトテキストビルド
        /// </summary>
        StringBuilder BuildScriptText(PlantUMLConvertOption option);

    }
}


namespace UML
{

    /// <summary>
    /// コンテンツパーサーインターフェース
    /// </summary>
    public interface IContentParser
    {
        /// <summary>
        /// パース処理
        /// </summary>
        /// <param name="lines">行毎の文字列</param>
        /// <param name="index">開始インデックス</param>
        /// <param name="namespace_name">ネームスペース名</param>
        IContentBuilder[] Parse(string[] lines, ref int index, string namespace_name = "");
    }
}
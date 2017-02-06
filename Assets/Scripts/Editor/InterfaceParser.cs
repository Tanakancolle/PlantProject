using System.Collections.Generic;


public class InterfaceParser : ParserBase {
    public override StructuralInfoBase[] Parse (string[] lines, ref int index,string namespace_name = "")
    {
        var words = SplitSpace (lines [index]);

        // インターフェースチェック
        if (!CheckWord (words, "interface")) {
            return null;
        }
        
        var info = new InterfaceInfo ();

        // インターフェース名設定
        info.structuralName = lines [index].Replace ("interface", string.Empty).Replace ("{", string.Empty).Trim ();

        // 内容までインデックスをずらす
        index++;
        if (lines [index].IndexOf ("{") >= 0) {
            index++;
        }

        // 定義終了まで内容をパース
        info.menberList = new List<MenberInfo> ();
        while (lines [index].IndexOf ("}") < 0) {
            var menber = new MenberInfo ();

            menber.name = string.Format ("public {0}", lines [index].TrimStart ());
            menber.isAbstract = true;

            info.menberList.Add (menber);

            index++;
        }

        index++;

        return new StructuralInfoBase[] { info };
    }
}

using System.Collections.Generic;


public class ClassParser : ParserBase {
    public override StructuralInfoBase[] Parse (string[] lines, ref int index, string namespace_name = "")
    {
        var words = SplitSpace (lines [index]);
        
        // クラスチェック
        if (!CheckWord (words, "class")) {
            return null;
        }

        var info = new ClassInfo ();

        // クラス名設定
        info.structuralName = lines [index].Replace ("class", string.Empty).Replace ("{", string.Empty).Replace ("abstract", string.Empty).Trim ();

        // 抽象クラスフラグ設定
        info.isAbstract = CheckWord (SplitSpace (lines [index]), "abstract");

        // 内容までインデックスをずらす
        index++;
        if (lines [index].IndexOf ("{") >= 0) {
            index++;
        }

        // 定義終了まで内容をパース
        info.menberList = new List<MenberInfo> ();
        while (lines [index].IndexOf ("}") < 0) {
            var menber = new MenberInfo ();

            menber.name = ReplaceAccessModifiers (lines [index]).TrimStart ();
            menber.isAbstract = CheckWord (SplitSpace (lines [index]), "abstract");

            info.menberList.Add (menber);

            index++;
        }

        index++;

        return new StructuralInfoBase[] { info };
    }
}

using System.Linq;
using System.Text;

/// <summary>
/// クラス情報
/// </summary>
public class ClassInfo : StructuralInfoBase {

    /// <summary>
    /// 抽象クラスフラグ
    /// </summary>
    public bool isAbstract = false;

    public override string GetDeclarationName()
    {
        var declaration = string.Empty;

        // 抽象クラスチェック
        if (isAbstract) {
            declaration = string.Format("public abstract class {0}", structuralName);
        } else {
            declaration = string.Format("public class {0}", structuralName);
        }

        // 継承チェック
        if (inheritanceList != null && inheritanceList.Count > 0)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var inheritance in inheritanceList.Select(x => x.GetName()))
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(inheritance);
            }

            declaration = string.Format("{0} : {1}", declaration, builder.ToString());
        }

        return declaration;
    }
}
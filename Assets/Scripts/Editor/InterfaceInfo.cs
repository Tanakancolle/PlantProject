/// <summary>
/// インターフェース情報
/// </summary>
public class InterfaceInfo : ContentInfoBase {
    public override string GetDeclarationName ()
    {
        return string.Format ("public interface {0}", contentName);
    }
}
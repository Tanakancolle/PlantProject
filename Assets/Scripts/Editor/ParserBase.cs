using System.Collections.Generic;

public abstract class ParserBase {
    

    public abstract StructuralInfoBase[] Parse (string[] lines, ref int index, string namespace_name = "");


} 
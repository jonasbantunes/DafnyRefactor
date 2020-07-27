namespace Microsoft.Dafny
{
    public class InlineVar
    {
        public string name => tableDeclaration.name;
        public Expression expr = null;
        public bool isUpdated = false;
        public SymbolTableDeclaration tableDeclaration = null;
    }
}

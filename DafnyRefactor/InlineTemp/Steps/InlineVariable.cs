namespace Microsoft.Dafny
{
    public class InlineVariable
    {
        public string Name => tableDeclaration.name;
        public Expression expr = null;
        public bool isUpdated = false;
        public SymbolTableDeclaration tableDeclaration = null;
    }
}

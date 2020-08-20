namespace Microsoft.Dafny
{
    public class InlineVariable
    {
        public Expression expr = null;
        public bool isUpdated = false;
        public UpdateStmt initStmt = null;
        public string Name => TableDeclaration.Name;
        public SymbolTableDeclaration TableDeclaration { get; protected set; } = null;

        public InlineVariable(SymbolTableDeclaration tableDeclaration)
        {
            TableDeclaration = tableDeclaration;
        }
    }
}

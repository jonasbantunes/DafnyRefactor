namespace Microsoft.Dafny
{
    public class SymbolTableDeclaration
    {
        protected int hashCode;
        public LocalVariable LocalVariable { get; protected set; }
        public VarDeclStmt VarDeclStmt { get; protected set; }
        public string Name => LocalVariable.Name;

        public SymbolTableDeclaration(LocalVariable localVariable, VarDeclStmt varDeclStmt)
        {
            LocalVariable = localVariable;
            VarDeclStmt = varDeclStmt;
            hashCode = localVariable.GetHashCode();
        }

        public override int GetHashCode()
        {
            return hashCode;
        }
    }
}

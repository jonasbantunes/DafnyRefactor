using Microsoft.Dafny;

namespace Microsoft.DafnyRefactor.Utils
{
    public interface ISymbol
    {
        LocalVariable LocalVariable { get; }
        VarDeclStmt VarDeclStmt { get; }
        string Name { get; }
    }

    public class Symbol : ISymbol
    {
        protected readonly LocalVariable localVariable;
        protected VarDeclStmt varDeclStmt;

        public Symbol(LocalVariable localVariable, VarDeclStmt varDeclStmt)
        {
            this.localVariable = localVariable;
            this.varDeclStmt = varDeclStmt;
        }

        public LocalVariable LocalVariable => localVariable;
        public VarDeclStmt VarDeclStmt => varDeclStmt;
        public string Name => LocalVariable.Name;

        public override int GetHashCode()
        {
            // TODO: Check if is better to use localVariable or localVariable.Tok
            return LocalVariable?.GetHashCode() ?? 0;
        }

        public static Symbol CreateSymbol(LocalVariable localVariable, VarDeclStmt varDeclStmt)
        {
            return new Symbol(localVariable, varDeclStmt);
        }
    }
}
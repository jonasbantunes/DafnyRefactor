using Microsoft.Dafny;

namespace DafnyRefactor.Utils.SymbolTable
{
    public class SymbolTableDeclaration
    {
        public readonly LocalVariable localVariable;
        public readonly VarDeclStmt varDeclStmt;
        public string Name => localVariable.Name;

        public SymbolTableDeclaration(LocalVariable localVariable, VarDeclStmt varDeclStmt)
        {
            this.localVariable = localVariable;
            this.varDeclStmt = varDeclStmt;
        }

        public override int GetHashCode()
        {
            return localVariable.GetHashCode();
        }
    }
}
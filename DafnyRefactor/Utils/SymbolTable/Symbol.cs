using Microsoft.Dafny;

namespace DafnyRefactor.Utils.SymbolTable
{
    public class Symbol
    {
        public readonly LocalVariable localVariable;
        public readonly VarDeclStmt varDeclStmt;
        public string Name => localVariable.Name;

        public Symbol(LocalVariable localVariable, VarDeclStmt varDeclStmt)
        {
            this.localVariable = localVariable;
            this.varDeclStmt = varDeclStmt;
        }

        public override int GetHashCode()
        {
            return localVariable.GetHashCode();
        }

        public static Symbol CreateSymbol(LocalVariable localVariable, VarDeclStmt varDeclStmt)
        {
            return new Symbol(localVariable, varDeclStmt);
        }
    }
}
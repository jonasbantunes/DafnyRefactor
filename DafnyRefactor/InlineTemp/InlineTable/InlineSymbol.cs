using DafnyRefactor.Utils.SymbolTable;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp.InlineTable
{
    public class InlineSymbol : Symbol
    {
        public bool isUpdated = false;
        public Expression expr;

        public InlineSymbol(LocalVariable localVariable, VarDeclStmt varDeclStmt) : base(localVariable, varDeclStmt)
        {
        }

        public bool IsConstant()
        {
            return (expr != null) && !isUpdated;
        }

        public static InlineSymbol CreateInlineSymbol(LocalVariable localVariable, VarDeclStmt varDeclStmt)
        {
            return new InlineSymbol(localVariable, varDeclStmt);
        }
    }
}
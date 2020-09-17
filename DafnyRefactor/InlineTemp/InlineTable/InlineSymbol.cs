using DafnyRefactor.Utils.SymbolTable;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp.InlineTable
{
    public interface IInlineSymbol : ISymbol
    {
        bool IsUpdated { get; set; }
        Expression Expr { get; set; }
        UpdateStmt InitStmt { get; set; }

        bool IsConstant();
    }

    public class InlineSymbol : Symbol, IInlineSymbol
    {
        public InlineSymbol(LocalVariable localVariable, VarDeclStmt varDeclStmt) : base(localVariable, varDeclStmt)
        {
        }

        public bool IsUpdated { get; set; } = false;

        public Expression Expr { get; set; }

        // TODO: Give a better name
        public UpdateStmt InitStmt { get; set; } = null;

        public bool IsConstant()
        {
            return Expr != null && !IsUpdated;
        }

        public static InlineSymbol CreateInlineSymbol(LocalVariable localVariable, VarDeclStmt varDeclStmt)
        {
            return new InlineSymbol(localVariable, varDeclStmt);
        }
    }
}
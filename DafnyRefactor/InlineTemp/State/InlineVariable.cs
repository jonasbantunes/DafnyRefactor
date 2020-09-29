using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     Represents a variable from AST that is used during the refactor process.
    ///     <para>
    ///         This specialized version of <c>IRefactorVariable</c> contains data neccessary to verify if
    ///         is possible to refactor this variable.
    ///     </para>
    /// </summary>
    public interface IInlineVariable : IRefactorVariable
    {
        Expression Expr { get; set; }
        UpdateStmt InitStmt { get; set; }
        bool IsUpdated { get; set; }
        bool NotAnExpr { get; set; }

        bool IsConstant();
    }

    public class InlineVariable : RefactorVariable, IInlineVariable
    {
        public InlineVariable(LocalVariable localVariable, VarDeclStmt varDeclStmt) : base(localVariable, varDeclStmt)
        {
        }

        public bool IsUpdated { get; set; } = false;
        public bool NotAnExpr { get; set; } = false;

        public Expression Expr { get; set; }

        // TODO: Give a better name
        public UpdateStmt InitStmt { get; set; } = null;

        public bool IsConstant()
        {
            return Expr != null && !IsUpdated;
        }
    }
}
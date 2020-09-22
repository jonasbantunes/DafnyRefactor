﻿using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    public interface IInlineVariable : IRefactorVariable
    {
        bool IsUpdated { get; set; }
        Expression Expr { get; set; }
        UpdateStmt InitStmt { get; set; }

        bool IsConstant();
    }

    public class InlineVariable : RefactorVariable, IInlineVariable
    {
        public InlineVariable(LocalVariable localVariable, VarDeclStmt varDeclStmt) : base(localVariable, varDeclStmt)
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
    }
}
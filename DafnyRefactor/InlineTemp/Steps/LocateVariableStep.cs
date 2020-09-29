using System;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     A <c>RefactorStep</c> that locate the variable to be refactored.
    /// </summary>
    public class LocateVariableStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Errors == null || state.InlineOptions == null || state.Program == null ||
                state.RootScope == null) throw new ArgumentNullException();

            var locator = new VariableLocator(state.Program, state.RootScope, state.InlineOptions.VarLine,
                state.InlineOptions.VarColumn);
            locator.Execute();
            if (locator.FoundDeclaration == null)
            {
                state.AddError(
                    $"Error: can't locate variable on line {state.InlineOptions.VarLine}:{state.InlineOptions.VarColumn}.");
                return;
            }

            state.InlineVariable = locator.FoundDeclaration;

            base.Handle(state);
        }
    }

    internal class VariableLocator : DafnyVisitorWithNearests
    {
        protected Program program;
        protected IInlineScope rootScope;
        protected int varColumn;
        protected int varLine;

        public VariableLocator(Program program, IInlineScope rootScope, int varLine, int varColumn)
        {
            if (program == null || rootScope == null) throw new ArgumentNullException();

            this.program = program;
            this.varLine = varLine;
            this.varColumn = varColumn;
            this.rootScope = rootScope;
        }

        public IInlineVariable FoundDeclaration { get; protected set; }
        protected IInlineScope CurTable => rootScope.FindInlineScope(nearestScopeToken.GetHashCode());

        public virtual void Execute()
        {
            Visit(program);
        }

        protected override void Visit(VarDeclStmt vds)
        {
            foreach (var local in vds.Locals)
            {
                if (!IsInRange(varLine, varColumn, local.Tok.line, local.Tok.col, local.EndTok.line,
                    local.EndTok.col)) continue;

                FoundDeclaration = CurTable.LookupInlineVariable(local.Name);
            }

            base.Visit(vds);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            if (!IsInRange(varLine, varColumn, nameSeg.tok.line, nameSeg.tok.col, nameSeg.tok.line,
                nameSeg.tok.col + nameSeg.tok.val.Length - 1)) return;

            FoundDeclaration = CurTable.LookupInlineVariable(nameSeg.Name);
        }

        protected override void Visit(IdentifierExpr identifierExpr)
        {
            if (!IsInRange(varLine, varColumn, identifierExpr.tok.line, identifierExpr.tok.col, identifierExpr.tok.line,
                identifierExpr.tok.col + identifierExpr.tok.val.Length - 1)) return;

            FoundDeclaration = CurTable.LookupInlineVariable(identifierExpr.Name);

            base.Visit(identifierExpr);
        }

        protected override void Visit(ExprDotName exprDotName)
        {
        }

        protected override void Visit(MemberSelectExpr memmMemberSelectExpr)
        {
        }

        protected bool IsInRange(int line, int column, int startLine, int starColumn, int endLine, int endColumn)
        {
            if (startLine != line || starColumn > column) return false;
            if ((startLine >= line || line >= endLine) && (line != endLine || column > endColumn)) return false;
            return true;
        }
    }
}
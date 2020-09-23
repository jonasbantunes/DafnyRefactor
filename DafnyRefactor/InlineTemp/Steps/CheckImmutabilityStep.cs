using System;
using System.Linq;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    public class CheckImmutabilityStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.InlineVariable == null || state.Program == null || state.RootScope == null)
                throw new ArgumentNullException();

            var visitor = new InlineRetrieveVisitor(state.Program, state.RootScope);
            visitor.Execute();
            if (state.InlineVariable.Expr == null)
            {
                state.AddError(
                    $"Error: variable {state.InlineVariable.Name} located on {state.InlineOptions.VarLine}:{state.InlineOptions.VarColumn} is never initialized.");
                return;
            }

            if (state.InlineVariable.IsUpdated)
            {
                state.AddError(
                    $"Error: variable {state.InlineVariable.Name} located on {state.InlineOptions.VarLine}:{state.InlineOptions.VarColumn} is not constant.");
                return;
            }

            base.Handle(state);
        }
    }

    internal class InlineRetrieveVisitor : DafnyVisitorWithNearests
    {
        protected Program program;
        protected IInlineScope rootScope;

        public InlineRetrieveVisitor(Program program, IInlineScope rootScope)
        {
            if (program == null || rootScope == null) throw new ArgumentNullException();

            this.program = program;
            this.rootScope = rootScope;
        }

        public virtual void Execute()
        {
            Visit(program);
        }

        protected override void Visit(VarDeclStmt vds)
        {
            if (vds == null) throw new ArgumentNullException();

            if (!(vds.Update is UpdateStmt up)) return;
            for (var i = 0; i < up.Lhss.Count; i++)
            {
                if (!(up.Lhss[i] is AutoGhostIdentifierExpr agie)) continue;
                // TODO: Avoid this repetition on source code
                var curTable = rootScope.FindInlineScope(nearestScopeToken.GetHashCode());
                var symbol = curTable.LookupInlineSymbol(agie.Name);
                if (symbol == null) continue;
                var assign = up.Rhss[i];
                symbol.Expr = assign.SubExpressions.FirstOrDefault();
                symbol.InitStmt = up;
            }
        }

        protected override void Visit(UpdateStmt up)
        {
            if (up?.Lhss == null) throw new ArgumentNullException();

            for (var i = 0; i < up.Lhss.Count; i++)
            {
                if (!(up.Lhss[i] is NameSegment nm)) continue;
                var curTable = rootScope.FindInlineScope(nearestScopeToken.GetHashCode());
                var symbol = curTable.LookupInlineSymbol(nm.Name);
                if (symbol.Expr == null && up.Rhss[i] is ExprRhs erhs)
                {
                    symbol.Expr = erhs.Expr;
                    symbol.InitStmt = up;
                }
                else
                {
                    symbol.IsUpdated = true;
                }
            }
        }
    }
}
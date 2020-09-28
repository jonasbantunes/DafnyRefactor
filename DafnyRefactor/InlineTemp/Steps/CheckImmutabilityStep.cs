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

            var retriever = new InlineRetrieveVisitor(state.Program, state.RootScope);
            retriever.Execute();

            if (state.InlineVariable.IsUpdated)
            {
                state.AddError(
                    $"Error: variable {state.InlineVariable.Name} located on {state.InlineOptions.VarLine}:{state.InlineOptions.VarColumn} is not constant.");
                return;
            }

            if (state.InlineVariable.NotAnExpr)
            {
                state.AddError(
                    $"Error: variable {state.InlineVariable.Name} located on {state.InlineOptions.VarLine}:{state.InlineOptions.VarColumn} is initialized with an object constructor.");
                return;
            }

            if (state.InlineVariable.Expr == null)
            {
                state.AddError(
                    $"Error: variable {state.InlineVariable.Name} located on {state.InlineOptions.VarLine}:{state.InlineOptions.VarColumn} is never initialized.");
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

        protected IInlineScope CurTable => rootScope.FindInlineScope(nearestScopeToken.GetHashCode());

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

                var variable = CurTable.LookupInlineVariable(agie.Name);
                if (variable == null) continue;

                var assign = up.Rhss[i];
                var expr = assign.SubExpressions.FirstOrDefault();
                if (expr == null)
                {
                    variable.NotAnExpr = true;
                }
                else
                {
                    variable.Expr = expr;
                }

                variable.InitStmt = up;
            }
        }

        protected override void Visit(UpdateStmt up)
        {
            if (up?.Lhss == null) throw new ArgumentNullException();

            for (var i = 0; i < up.Lhss.Count; i++)
            {
                if (!(up.Lhss[i] is NameSegment nm)) continue;

                var variable = CurTable.LookupInlineVariable(nm.Name);
                if (variable == null) continue;

                if (variable.InitStmt == null && up.Rhss[i] is ExprRhs erhs)
                {
                    variable.Expr = erhs.Expr;
                    variable.InitStmt = up;
                }
                else
                {
                    variable.IsUpdated = true;
                }
            }
        }
    }
}
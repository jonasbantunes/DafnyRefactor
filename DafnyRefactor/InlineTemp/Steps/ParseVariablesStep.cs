using System;
using System.Linq;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     A <c>RefactorStep</c> that parses all variables of a <c>Dafny.Program</c> into <c>RefactorVariable</c>
    ///     and save each variable into it's respective <c>RefactorState</c>.
    /// </summary>
    public class ParseVariablesStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null || state.RootScope == null)
                throw new ArgumentNullException();

            var retriever = new VariableParser(state.Program, state.RootScope);
            retriever.Execute();

            base.Handle(state);
        }
    }

    internal class VariableParser : DafnyVisitorWithNearests
    {
        protected Program program;
        protected IInlineScope rootScope;

        public VariableParser(Program program, IInlineScope rootScope)
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
            if (!(vds.Update is UpdateStmt up)) return;
            for (var i = 0; i < up.Lhss.Count; i++)
            {
                if (!(up.Lhss[i] is AutoGhostIdentifierExpr agie)) continue;

                var variable = CurTable.LookupInlineVariable(agie.Name);
                if (variable == null) continue;

                UpdateVariable(variable, up.Rhss[i], up);
            }
        }

        protected override void Visit(UpdateStmt up)
        {
            for (var i = 0; i < up.Lhss.Count; i++)
            {
                if (!(up.Lhss[i] is NameSegment nm)) continue;

                var variable = CurTable.LookupInlineVariable(nm.Name);
                if (variable == null) continue;

                if (variable.InitStmt == null)
                {
                    UpdateVariable(variable, up.Rhss[i], up);
                }
                else
                {
                    variable.IsUpdated = true;
                }
            }
        }

        protected virtual void UpdateVariable(IInlineVariable variable, AssignmentRhs assign, UpdateStmt initStmt)
        {
            var expr = assign.SubExpressions.FirstOrDefault();
            if (expr == null)
            {
                variable.NotAnExpr = true;
            }
            else
            {
                variable.Expr = expr;
            }

            variable.InitStmt = initStmt;
        }
    }
}
using System;
using System.Linq;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp
{
    public class VariableParser : DafnyVisitorWithNearests
    {
        private readonly Program _program;
        private readonly IInlineScope _rootScope;

        private VariableParser(Program program, IInlineScope rootScope)
        {
            if (program == null || rootScope == null) throw new ArgumentNullException();

            _program = program;
            _rootScope = rootScope;
        }

        private IInlineScope CurTable => _rootScope.FindInlineScope(nearestScopeToken.GetHashCode());

        private void Execute()
        {
            Visit(_program);
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

        private void UpdateVariable(IInlineVariable variable, AssignmentRhs assign, UpdateStmt initStmt)
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

        public static void Parse(Program program, IInlineScope rootScope)
        {
            var parser = new VariableParser(program, rootScope);
            parser.Execute();
        }
    }
}
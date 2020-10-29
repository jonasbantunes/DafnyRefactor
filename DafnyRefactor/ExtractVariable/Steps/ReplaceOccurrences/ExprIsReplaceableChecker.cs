using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public class ExprIsReplaceableChecker : DafnyVisitorWithNearests
    {
        protected Range expRange;
        protected bool isReplaceable;
        protected Program program;
        protected IRefactorScope rootScope;
        protected List<IRefactorVariable> variables;

        protected ExprIsReplaceableChecker(Program program, Range expRange, IRefactorScope rootScope,
            List<IRefactorVariable> variables)
        {
            this.program = program;
            this.expRange = expRange;
            this.rootScope = rootScope;
            this.variables = variables;
        }

        protected void Execute()
        {
            isReplaceable = true;
            Visit(program);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            if (expRange.start > nameSeg.tok.pos || nameSeg.tok.pos > expRange.end) return;

            var curScope = rootScope.FindScope(nearestScopeToken.GetHashCode());
            if (curScope == null) return;

            var variable = curScope.LookupVariable(nameSeg.Name);
            if (variable == null) return;

            var found = variables.FindIndex(v => v.Equals(variable));
            if (found == -1)
            {
                isReplaceable = false;
                return;
            }

            base.Visit(nameSeg);
        }

        public static bool IsReplacable(Program program, Range expRange, IRefactorScope rootScope,
            List<IRefactorVariable> variables)
        {
            var checker = new ExprIsReplaceableChecker(program, expRange, rootScope, variables);
            checker.Execute();
            return checker.isReplaceable;
        }
    }
}
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.ExtractVariable
{
    public class ExprIsReplaceableChecker : DafnyVisitorWithNearests
    {
        private readonly Range _expRange;
        private readonly Program _program;
        private readonly IRefactorScope _rootScope;
        private readonly List<IRefactorVariable> _variables;
        private bool _isReplaceable;

        private ExprIsReplaceableChecker(Program program, Range expRange, IRefactorScope rootScope,
            List<IRefactorVariable> variables)
        {
            _program = program;
            _expRange = expRange;
            _rootScope = rootScope;
            _variables = variables;
        }

        private void Execute()
        {
            _isReplaceable = true;
            Visit(_program);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            if (_expRange.start > nameSeg.tok.pos || nameSeg.tok.pos > _expRange.end) return;

            var curScope = _rootScope.FindScope(nearestScopeToken.GetHashCode());
            if (curScope == null) return;

            var variable = curScope.LookupVariable(nameSeg.Name);
            if (variable == null) return;

            var found = _variables.FindIndex(v => v.Equals(variable));
            if (found == -1)
            {
                _isReplaceable = false;
                return;
            }

            base.Visit(nameSeg);
        }

        public static bool IsReplacable(Program program, Range expRange, IRefactorScope rootScope,
            List<IRefactorVariable> variables)
        {
            var checker = new ExprIsReplaceableChecker(program, expRange, rootScope, variables);
            checker.Execute();
            return checker._isReplaceable;
        }
    }
}
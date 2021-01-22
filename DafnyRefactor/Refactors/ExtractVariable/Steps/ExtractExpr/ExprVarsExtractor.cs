using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.ExtractVariable
{
    public class ExprVarsExtractor : DafnyVisitorWithNearests
    {
        private readonly Range _exprRange;
        private readonly Program _program;
        private readonly IRefactorScope _rootScope;
        private List<IRefactorVariable> _variables;

        private ExprVarsExtractor(Program program, Range exprRange, IRefactorScope rootScope)
        {
            _program = program;
            _exprRange = exprRange;
            _rootScope = rootScope;
        }

        private void Execute()
        {
            _variables = new List<IRefactorVariable>();
            Visit(_program);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            if (_exprRange.start > nameSeg.tok.pos || nameSeg.tok.pos > _exprRange.end) return;

            var curScope = _rootScope.FindScope(nearestScopeToken.GetHashCode());
            if (curScope == null) return;

            var variable = curScope.LookupVariable(nameSeg.Name);
            if (variable == null) return;

            _variables.Add(variable);

            base.Visit(nameSeg);
        }

        public static List<IRefactorVariable> Extract(Program program, Range expRange, IRefactorScope rootScope)
        {
            var extractor = new ExprVarsExtractor(program, expRange, rootScope);
            extractor.Execute();
            return extractor._variables;
        }
    }
}
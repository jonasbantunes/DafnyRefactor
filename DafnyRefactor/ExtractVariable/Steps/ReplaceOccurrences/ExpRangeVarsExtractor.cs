using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public class ExpRangeVarsExtractor : DafnyVisitorWithNearests
    {
        protected Range expRange;
        protected Program program;
        protected IRefactorScope rootScope;
        protected List<IRefactorVariable> variables;

        protected ExpRangeVarsExtractor(Program program, Range expRange, IRefactorScope rootScope)
        {
            this.program = program;
            this.expRange = expRange;
            this.rootScope = rootScope;
        }

        protected void Execute()
        {
            variables = new List<IRefactorVariable>();
            Visit(program);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            if (expRange.start > nameSeg.tok.pos || nameSeg.tok.pos > expRange.end) return;

            var curScope = rootScope.FindScope(nearestScopeToken.GetHashCode());
            if (curScope == null) return;

            var variable = curScope.LookupVariable(nameSeg.Name);
            if (variable == null) return;

            variables.Add(variable);

            base.Visit(nameSeg);
        }

        public static List<IRefactorVariable> Extract(Program program, Range expRange, IRefactorScope rootScope)
        {
            var extractor = new ExpRangeVarsExtractor(program, expRange, rootScope);
            extractor.Execute();
            return extractor.variables;
        }
    }
}
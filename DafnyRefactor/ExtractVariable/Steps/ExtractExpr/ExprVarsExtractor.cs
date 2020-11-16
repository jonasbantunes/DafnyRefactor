using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.ExtractVariable
{
    public class ExprVarsExtractor : DafnyVisitorWithNearests
    {
        protected Range exprRange;
        protected Program program;
        protected IRefactorScope rootScope;
        protected List<IRefactorVariable> variables;

        protected ExprVarsExtractor(Program program, Range exprRange, IRefactorScope rootScope)
        {
            this.program = program;
            this.exprRange = exprRange;
            this.rootScope = rootScope;
        }

        protected void Execute()
        {
            variables = new List<IRefactorVariable>();
            Visit(program);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            if (exprRange.start > nameSeg.tok.pos || nameSeg.tok.pos > exprRange.end) return;

            var curScope = rootScope.FindScope(nearestScopeToken.GetHashCode());
            if (curScope == null) return;

            var variable = curScope.LookupVariable(nameSeg.Name);
            if (variable == null) return;

            variables.Add(variable);

            base.Visit(nameSeg);
        }

        public static List<IRefactorVariable> Extract(Program program, Range expRange, IRefactorScope rootScope)
        {
            var extractor = new ExprVarsExtractor(program, expRange, rootScope);
            extractor.Execute();
            return extractor.variables;
        }
    }
}
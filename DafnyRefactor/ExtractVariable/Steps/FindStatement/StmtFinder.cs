using System;
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.ExtractVariable
{
    public class StmtFinder : DafnyVisitorWithNearests
    {
        protected Statement foundStmt;
        protected Program program;
        protected IExtractVariableScope rootScope;
        protected Range selection;
        protected List<int> stmtDivisors;

        protected StmtFinder(Program program, Range selection, List<int> stmtDivisors, IExtractVariableScope rootScope)
        {
            if (program == null || selection == null || stmtDivisors == null || rootScope == null)
                throw new ArgumentNullException();

            this.program = program;
            this.selection = selection;
            this.stmtDivisors = stmtDivisors;
            this.rootScope = rootScope;
        }

        public void Execute()
        {
            Visit(program);
        }

        protected override void Visit(Statement stmt)
        {
            if (!(stmt is AssignStmt) && StmtContainsSelection(stmt))
            {
                var curScope = rootScope.EvrFindScope(nearestScopeToken.GetHashCode());
                if (curScope == null) return;
                foundStmt = stmt;
                curScope.CanReplace = true;
            }

            base.Visit(stmt);
        }

        protected bool StmtContainsSelection(Statement stmt)
        {
            var divisorIndex = stmtDivisors.FindIndex(divisor => divisor > stmt.Tok.pos);
            if (divisorIndex < 1) return false;

            var stmtStart = stmtDivisors[divisorIndex - 1];
            var stmtEnd = stmtDivisors[divisorIndex];

            return stmtStart <= selection.start && selection.end <= stmtEnd;
        }

        public static Statement Find(Program program, Range selection, List<int> stmtDivisors,
            IExtractVariableScope rootScope)
        {
            var finder = new StmtFinder(program, selection, stmtDivisors, rootScope);
            finder.Execute();
            return finder.foundStmt;
        }
    }
}
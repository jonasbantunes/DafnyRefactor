using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public class StmtFinder : DafnyVisitorWithNearests
    {
        protected Statement foundStmt;
        protected Program program;
        protected Range selection;
        protected List<int> stmtDivisors;

        protected StmtFinder(Program program, Range selection, List<int> stmtDivisors)
        {
            this.program = program;
            this.selection = selection;
            this.stmtDivisors = stmtDivisors;
        }

        public void Execute()
        {
            Visit(program);
        }

        protected override void Visit(Statement stmt)
        {
            if (StmtContainsSelection(stmt))
            {
                foundStmt = stmt;
            }

            base.Visit(stmt);
        }

        protected bool StmtContainsSelection(Statement stmt)
        {
            var divisorIndex = stmtDivisors.FindIndex(divisor => divisor >= stmt.EndTok.pos);
            if (divisorIndex < 1) return false;

            var stmtStart = stmtDivisors[divisorIndex - 1];
            var stmtEnd = stmtDivisors[divisorIndex];

            return stmtStart <= selection.start && selection.end <= stmtEnd;
        }

        public static Statement Find(Program program, Range selection, List<int> stmtDivisors)
        {
            var finder = new StmtFinder(program, selection, stmtDivisors);
            finder.Execute();
            return finder.foundStmt;
        }
    }
}
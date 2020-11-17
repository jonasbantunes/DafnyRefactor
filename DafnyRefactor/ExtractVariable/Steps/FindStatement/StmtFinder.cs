using System;
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.ExtractVariable
{
    public class StmtFinder : DafnyVisitorWithNearests
    {
        private readonly Program _program;
        private readonly IExtractVariableScope _rootScope;
        private readonly Range _selection;
        private readonly List<int> _stmtDivisors;
        private Statement _foundStmt;

        private StmtFinder(Program program, Range selection, List<int> stmtDivisors, IExtractVariableScope rootScope)
        {
            if (program == null || selection == null || stmtDivisors == null || rootScope == null)
                throw new ArgumentNullException();

            _program = program;
            _selection = selection;
            _stmtDivisors = stmtDivisors;
            _rootScope = rootScope;
        }

        private void Execute()
        {
            Visit(_program);
        }

        protected override void Visit(Statement stmt)
        {
            if (!(stmt is AssignStmt) && StmtContainsSelection(stmt))
            {
                var curScope = _rootScope.EvrFindScope(nearestScopeToken.GetHashCode());
                if (curScope == null) return;
                _foundStmt = stmt;
                curScope.CanReplace = true;
            }

            base.Visit(stmt);
        }

        private bool StmtContainsSelection(Statement stmt)
        {
            var divisorIndex = _stmtDivisors.FindIndex(divisor => divisor > stmt.Tok.pos);
            if (divisorIndex < 1) return false;

            var stmtStart = _stmtDivisors[divisorIndex - 1];
            var stmtEnd = _stmtDivisors[divisorIndex];

            return stmtStart <= _selection.start && _selection.end <= stmtEnd;
        }

        public static Statement Find(Program program, Range selection, List<int> stmtDivisors,
            IExtractVariableScope rootScope)
        {
            var finder = new StmtFinder(program, selection, stmtDivisors, rootScope);
            finder.Execute();
            return finder._foundStmt;
        }
    }
}
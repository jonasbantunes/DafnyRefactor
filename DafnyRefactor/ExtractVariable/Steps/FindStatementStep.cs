using System;
using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.ExtractVariable;
using Microsoft.DafnyRefactor.Utils;

namespace DafnyRefactor.ExtractVariable.Steps
{
    public class FindStatementStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.FilePath == null || state.Range == null || state.StmtDivisors == null)
                throw new ArgumentNullException();

            var visitor = new FindStatementVisitor(state.Program, state.Range, state.StmtDivisors);
            visitor.Execute();

            base.Handle(state);
        }
    }

    internal class FindStatementVisitor : DafnyVisitorWithNearests
    {
        protected Program program;
        protected Range range;
        protected List<int> stmtDivisors;
        public Statement FoundStmt { get; protected set; }

        public FindStatementVisitor(Program program, Range range, List<int> stmtDivisors)
        {
            this.program = program;
            this.range = range;
            this.stmtDivisors = stmtDivisors;
        }

        public void Execute()
        {
            Visit(program);
        }

        protected override void Visit(Statement stmt)
        {
            if (IsInStmt(range, stmt, stmtDivisors))
            {
                FoundStmt = stmt;
            }

            base.Visit(stmt);
        }

        protected bool IsInStmt(Range range, Statement stmt, List<int> stmtDivisors)
        {
            var divisorIndex = stmtDivisors.FindIndex(divisor => divisor >= stmt.EndTok.pos);
            if (divisorIndex < 1) return false;

            var start = stmtDivisors[divisorIndex - 1];
            var end = stmtDivisors[divisorIndex];

            return start.CompareTo(range.start) <= 0 && end.CompareTo(range.end) >= 0;
        }
    }
}
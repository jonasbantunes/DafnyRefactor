using System;
using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    /// <summary>
    ///     A <c>RefactorStep</c> that finds the statement with informed selection.
    /// </summary>
    public class FindStatementStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.FilePath == null || state.EvUserSelection == null || state.StmtDivisors == null)
                throw new ArgumentNullException();

            var visitor = new FindStatementVisitor(state.Program, state.EvUserSelection, state.StmtDivisors);
            visitor.Execute();
            if (visitor.FoundStmt == null)
            {
                state.Errors.Add("Error: Couldn't find selected expression.");
                return;
            }

            state.EvStmt = visitor.FoundStmt;

            base.Handle(state);
        }
    }

    internal class FindStatementVisitor : DafnyVisitorWithNearests
    {
        protected Program program;
        protected Range selection;
        protected List<int> stmtDivisors;

        public FindStatementVisitor(Program program, Range selection, List<int> stmtDivisors)
        {
            this.program = program;
            this.selection = selection;
            this.stmtDivisors = stmtDivisors;
        }

        public Statement FoundStmt { get; protected set; }

        public void Execute()
        {
            Visit(program);
        }

        protected override void Visit(Statement stmt)
        {
            if (IsInStmt(stmt))
            {
                FoundStmt = stmt;
            }

            base.Visit(stmt);
        }

        // TODO: Choose a better name
        protected bool IsInStmt(Statement stmt)
        {
            var divisorIndex = stmtDivisors.FindIndex(divisor => divisor >= stmt.EndTok.pos);
            if (divisorIndex < 1) return false;

            var stmtStart = stmtDivisors[divisorIndex - 1];
            var stmtEnd = stmtDivisors[divisorIndex];

            return stmtStart <= selection.start && selection.end <= stmtEnd;
        }
    }
}
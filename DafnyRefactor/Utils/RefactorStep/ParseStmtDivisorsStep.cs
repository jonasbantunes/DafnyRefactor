using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dafny;

namespace Microsoft.DafnyRefactor.Utils
{
    /// <summary>
    ///     ParseVariables a <c>Dafny.Program</c> and generate a list of statement divisors (<c>StmtDivisors</c>).
    ///     <para>
    ///         A "divisor" is a source code position where a statement ends or where a block starts or ends.
    ///     </para>
    /// </summary>
    public class ParseStmtDivisorsStep<TState> : RefactorStep<TState> where TState : IRefactorState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null) throw new ArgumentNullException();

            var parser = new ParseStmtDivisors(state.Program);
            parser.Execute();
            state.StmtDivisors = parser.StmtDivisors.ToList();

            base.Handle(state);
        }
    }

    internal class ParseStmtDivisors : DafnyVisitor
    {
        protected Program program;

        public ParseStmtDivisors(Program program)
        {
            this.program = program ?? throw new ArgumentNullException();
        }

        public SortedSet<int> StmtDivisors { get; protected set; }

        public virtual void Execute()
        {
            StmtDivisors = new SortedSet<int>();
            Visit(program);
        }

        protected override void Visit(BlockStmt block)
        {
            StmtDivisors.Add(block.Tok.pos);
            StmtDivisors.Add(block.EndTok.pos);

            base.Visit(block);
        }

        protected override void Visit(Statement stmt)
        {
            if (stmt == null) return;

            if (stmt.EndTok.val == ";")
            {
                StmtDivisors.Add(stmt.EndTok.pos);
            }

            base.Visit(stmt);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dafny;

namespace Microsoft.DafnyRefactor.Utils
{
    public class ParseStmtDivisorsStep<TState> : RefactorStep<TState> where TState : IRefactorState
    {
        public override void Handle(TState state)
        {
            var divisor = new ParseStmtDivisors(state.Program);
            divisor.Execute();
            state.StmtDivisors = divisor.StmtDivisors.ToList();
            base.Handle(state);
        }
    }

    internal class ParseStmtDivisors : DafnyVisitor
    {
        public ParseStmtDivisors(Program program) : base(program)
        {
        }

        public SortedSet<int> StmtDivisors { get; protected set; }

        public override void Execute()
        {
            StmtDivisors = new SortedSet<int>();
            base.Execute();
        }

        protected override void Visit(BlockStmt block)
        {
            StmtDivisors.Add(block.Tok.pos);
            StmtDivisors.Add(block.EndTok.pos);

            base.Visit(block);
        }

        protected override void Visit(Statement stmt)
        {
            // TODO: Add contractors to remove this IF
            if (stmt == null) return;
            if (stmt.EndTok.val == ";")
            {
                StmtDivisors.Add(stmt.EndTok.pos);
            }

            base.Visit(stmt);
        }
    }
}
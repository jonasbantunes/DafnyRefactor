using System;
using System.Collections.Generic;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils
{
    public class StmtDivisorsParser : DafnyVisitor
    {
        private readonly Program _program;

        private SortedSet<int> _stmtDivisors;

        private StmtDivisorsParser(Program program)
        {
            _program = program ?? throw new ArgumentNullException();
        }

        private void Execute()
        {
            _stmtDivisors = new SortedSet<int>();
            Visit(_program);
        }

        protected override void Visit(BlockStmt block)
        {
            _stmtDivisors.Add(block.Tok.pos);
            _stmtDivisors.Add(block.EndTok.pos);

            base.Visit(block);
        }

        protected override void Visit(Statement stmt)
        {
            if (stmt == null) return;

            if (stmt.EndTok.val == ";")
            {
                _stmtDivisors.Add(stmt.EndTok.pos);
            }

            base.Visit(stmt);
        }

        public static SortedSet<int> Parse(Program program)
        {
            var parser = new StmtDivisorsParser(program);
            parser.Execute();
            return parser._stmtDivisors;
        }
    }
}
using System;
using Microsoft.Dafny;

namespace Microsoft.DafnyRefactor.Utils
{
    public class ScopeGenerator<TScopeState> : DafnyVisitorWithNearests where TScopeState : IRefactorScope, new()
    {
        protected Program program;

        public ScopeGenerator(Program program)
        {
            this.program = program ?? throw new ArgumentNullException();
        }

        public TScopeState GeneratedTable { get; protected set; }

        public virtual void Execute()
        {
            GeneratedTable = new TScopeState();
            Visit(program);
        }

        protected override void Visit(ClassDecl cd)
        {
            if (cd == null) throw new ArgumentNullException();

            var curTable = GeneratedTable.FindScope(nearestScopeToken?.GetHashCode() ?? 0);
            curTable.InsertScope(cd.tok);

            base.Visit(cd);
        }

        protected override void Visit(VarDeclStmt vds)
        {
            if (vds == null) throw new ArgumentNullException();

            foreach (var local in vds.Locals)
            {
                var curTable = GeneratedTable.FindScope(nearestScopeToken?.GetHashCode() ?? 0);
                curTable.InsertVariable(local, vds);
            }
        }

        protected override void Visit(BlockStmt block)
        {
            if (block == null) throw new ArgumentNullException();

            var curTable = GeneratedTable.FindScope(nearestScopeToken?.GetHashCode() ?? 0);
            curTable.InsertScope(block.Tok);

            base.Visit(block);
        }
    }
}
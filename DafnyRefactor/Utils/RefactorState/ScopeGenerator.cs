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

        public TScopeState GeneratedScope { get; protected set; }

        public virtual void Execute()
        {
            GeneratedScope = new TScopeState();
            Visit(program);
        }

        protected override void Visit(ClassDecl cd)
        {
            var curTable = GeneratedScope.FindScope(nearestScopeToken?.GetHashCode() ?? 0);
            curTable.InsertScope(cd.tok);

            base.Visit(cd);
        }

        protected override void Visit(VarDeclStmt vds)
        {
            foreach (var local in vds.Locals)
            {
                var curTable = GeneratedScope.FindScope(nearestScopeToken?.GetHashCode() ?? 0);
                curTable.InsertVariable(local, vds);
            }
        }

        protected override void Visit(BlockStmt block)
        {
            var curTable = GeneratedScope.FindScope(nearestScopeToken?.GetHashCode() ?? 0);
            curTable.InsertScope(block.Tok);

            base.Visit(block);
        }
    }
}
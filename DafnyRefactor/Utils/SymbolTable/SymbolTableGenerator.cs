using System;
using Microsoft.Dafny;

namespace Microsoft.DafnyRefactor.Utils
{
    public class SymbolTableGenerator<TSymbolTable> : DafnyVisitor where TSymbolTable : ISymbolTable, new()
    {
        protected Program program;

        public SymbolTableGenerator(Program program)
        {
            this.program = program ?? throw new ArgumentNullException();
        }

        public TSymbolTable GeneratedTable { get; protected set; }

        public virtual void Execute()
        {
            GeneratedTable = new TSymbolTable();
            Visit(program);
        }

        protected override void Visit(VarDeclStmt vds)
        {
            if (vds == null) throw new ArgumentNullException();

            foreach (var local in vds.Locals)
            {
                var curTable = GeneratedTable.FindTable(nearestBlockStmt?.Tok?.GetHashCode() ?? 0);
                curTable.InsertSymbol(local, vds);
            }
        }

        protected override void Visit(BlockStmt block)
        {
            var curTable = GeneratedTable.FindTable(nearestBlockStmt?.Tok?.GetHashCode() ?? 0);

            curTable.InsertTable(block);

            base.Visit(block);
        }
    }
}
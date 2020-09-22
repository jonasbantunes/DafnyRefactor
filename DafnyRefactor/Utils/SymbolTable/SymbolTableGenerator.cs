using System;
using Microsoft.Dafny;

namespace Microsoft.DafnyRefactor.Utils
{
    public class SymbolTableGenerator<TSymbolTable> : DafnyVisitor where TSymbolTable : ISymbolTable, new()
    {
        public SymbolTableGenerator(Program program) :
            base(program)
        {
        }

        public TSymbolTable GeneratedTable { get; protected set; }

        public override void Execute()
        {
            GeneratedTable = new TSymbolTable();
            base.Execute();
        }

        protected override void Visit(VarDeclStmt vds)
        {
            if (vds == null) throw new ArgumentNullException();

            foreach (var local in vds.Locals)
            {
                // TODO: Find a better way to fallback to rootTable
                var curTable = GeneratedTable.FindTable(nearestBlockStmt.Tok.GetHashCode()) ?? GeneratedTable;
                curTable.InsertSymbol(local, vds);
            }
        }

        protected override void Visit(BlockStmt block)
        {
            // TODO: Find a better way to fallback to rootTable
            var curTable = nearestBlockStmt == null
                ? GeneratedTable
                : GeneratedTable.FindTable(nearestBlockStmt.Tok.GetHashCode());

            curTable.InsertTable(block);

            base.Visit(block);
        }
    }
}
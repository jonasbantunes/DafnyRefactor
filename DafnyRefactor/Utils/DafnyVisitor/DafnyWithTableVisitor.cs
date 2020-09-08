using DafnyRefactor.Utils.SymbolTable;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils.DafnyVisitor
{
    public class DafnyWithTableVisitor<TSymbol> : DafnyVisitor where TSymbol: Symbol
    {
        protected SymbolTable<TSymbol> curTable;
        protected SymbolTable<TSymbol> rootTable;

        public DafnyWithTableVisitor(Program program, SymbolTable<TSymbol> rootTable) : base(program)
        {
            this.rootTable = rootTable;
        }

        public override void Execute()
        {
            curTable = rootTable;
            base.Execute();
        }

        protected override void Visit(BlockStmt block)
        {
            curTable = curTable.LookupTable(block.Tok.GetHashCode());
            base.Visit(block);
            curTable = curTable.Parent;
        }
    }
}
using Microsoft.Dafny;

namespace DafnyRefactor.Utils.DafnyVisitor
{
    public class DafnyWithTableVisitor : DafnyVisitor
    {
        protected SymbolTable.SymbolTable curTable;
        protected SymbolTable.SymbolTable rootTable;

        public DafnyWithTableVisitor(Program program, SymbolTable.SymbolTable rootTable) : base(program)
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
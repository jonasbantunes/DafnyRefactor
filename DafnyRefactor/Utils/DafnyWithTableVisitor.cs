namespace Microsoft.Dafny
{
    public class DafnyWithTableVisitor : DafnyVisitor
    {
        protected SymbolTable curTable;
        protected SymbolTable rootTable;

        public DafnyWithTableVisitor(Program program, SymbolTable rootTable) : base(program)
        {
            this.rootTable = rootTable;
        }

        public override void Execute()
        {
            curTable = rootTable;
            base.Execute();
        }

        protected override BlockStmt Visit(BlockStmt block)
        {
            curTable = curTable.LookupTable(block.Tok.GetHashCode());
            var baseReturn =  base.Visit(block);
            curTable = curTable.parent;

            return baseReturn;
        }
    }
}

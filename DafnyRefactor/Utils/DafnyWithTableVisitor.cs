namespace Microsoft.Dafny
{
    public class DafnyWithTableVisitor : DafnyVisitor
    {
        protected SymbolTable curTable;
        protected readonly SymbolTable rootTable;

        public DafnyWithTableVisitor(Program program, SymbolTable rootTable) : base(program)
        {
            this.rootTable = rootTable;
        }

        public override void Execute()
        {
            curTable = rootTable;
            base.Execute();
        }

        protected override WhileStmt Visit(WhileStmt while_)
        {
            curTable = curTable.LookupTable(while_.Tok.GetHashCode());
            foreach (Statement stmt in while_.Body.Body)
            {
                Visit(stmt);
            }
            curTable = curTable.parent;

            return while_;
        }

    }
}

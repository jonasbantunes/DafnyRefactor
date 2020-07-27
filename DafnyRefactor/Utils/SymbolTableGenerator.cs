namespace Microsoft.Dafny
{
    public class SymbolTableGenerator : DafnyVisitor
    {
        protected SymbolTable curTable;
        public SymbolTable GeneratedTable { get; protected set; }

        public SymbolTableGenerator(Program program) : base(program) { }

        public override void Execute()
        {
            GeneratedTable = new SymbolTable();
            curTable = GeneratedTable;
            base.Execute();
        }

        protected override VarDeclStmt Visit(VarDeclStmt vds)
        {
            foreach (LocalVariable local in vds.Locals)
            {
                curTable.Insert(local.Tok);
            }

            return vds;
        }

        protected override WhileStmt Visit(WhileStmt while_)
        {
            var subTable = new SymbolTable
            {
                parent = curTable,
                hashCode = while_.Tok.GetHashCode()
            };
            curTable.Insert(subTable);
            curTable = subTable;

            foreach (Statement stmt in while_.Body.Body)
            {
                Visit(stmt);
            }

            curTable = curTable.parent;

            return while_;
        }
    }
}

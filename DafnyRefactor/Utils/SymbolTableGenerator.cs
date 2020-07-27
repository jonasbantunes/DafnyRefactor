namespace Microsoft.Dafny
{
    public class SymbolTableGenerator : DafnyVisitor
    {
        protected SymbolTable curTable;
        public SymbolTable table { get; protected set; }

        public override void execute()
        {
            table = new SymbolTable();
            curTable = table;
            base.execute();
        }

        protected override VarDeclStmt next(VarDeclStmt vds)
        {
            foreach (LocalVariable local in vds.Locals)
            {
                curTable.insert(local.Tok);
            }

            return vds;
        }

        protected override WhileStmt next(WhileStmt while_)
        {
            var subTable = new SymbolTable
            {
                parent = curTable,
                hashCode = while_.Tok.GetHashCode()
            };
            curTable.insert(subTable);
            curTable = subTable;

            foreach (Statement stmt in while_.Body.Body)
            {
                next(stmt);
            }

            curTable = curTable.parent;

            return while_;
        }
    }
}

namespace Microsoft.Dafny
{
    public class SymbolTableGenerator : DafnyProgramVisitor
    {
        public SymbolTable table;
        private SymbolTable curTable;

        public SymbolTableGenerator(Program program) : base(program)
        {
            table = new SymbolTable();
            curTable = table;
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

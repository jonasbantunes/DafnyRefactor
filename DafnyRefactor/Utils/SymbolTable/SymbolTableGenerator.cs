namespace Microsoft.Dafny
{
    public class SymbolTableGenerator : DafnyWithTableVisitor
    {
        public SymbolTable GeneratedTable { get; protected set; }

        // TODO: find better alternative than insert "null"
        public SymbolTableGenerator(Program program) : base(program, null) { }

        public override void Execute()
        {
            GeneratedTable = new SymbolTable();
            rootTable = GeneratedTable;
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

        protected override BlockStmt Visit(BlockStmt block)
        {
            var subTable = new SymbolTable
            {
                parent = curTable,
                hashCode = block.Tok.GetHashCode()
            };
            curTable.Insert(subTable);

            return base.Visit(block);
        }
    }
}

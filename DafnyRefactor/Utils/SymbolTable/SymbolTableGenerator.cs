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
                var declaration = new SymbolTableDeclaration(local, vds);
                curTable.InsertDeclaration(declaration);
            }

            return vds;
        }

        protected override BlockStmt Visit(BlockStmt block)
        {
            var subTable = new SymbolTable(block, curTable);
            curTable.InsertTable(subTable);

            return base.Visit(block);
        }
    }
}

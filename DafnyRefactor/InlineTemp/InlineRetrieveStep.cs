namespace Microsoft.Dafny
{
    public class InlineRetrieveStep : DafnyWithTableVisitor
    {
        protected SymbolTableDeclaration declaration;
        public InlineVariable InlineVar { get; protected set; }

        public InlineRetrieveStep(Program program, SymbolTable rootTable, SymbolTableDeclaration declaration) : base(program, rootTable)
        {
            this.declaration = declaration;
        }

        public override void Execute()
        {
            curTable = rootTable;
            InlineVar = new InlineVariable
            {
                tableDeclaration = declaration
            };

            base.Execute();
        }

        protected override VarDeclStmt Visit(VarDeclStmt vds)
        {
            if (vds.Update is UpdateStmt up)
            {
                for (int i = 0; i < up.Lhss.Count; i++)
                {
                    if (up.Lhss[i] is AutoGhostIdentifierExpr agie && agie.Name == InlineVar.Name && curTable.Lookup(agie.Name).GetHashCode() == InlineVar.tableDeclaration.GetHashCode())
                    {
                        ExprRhs erhs = (ExprRhs)up.Rhss[i];
                        InlineVar.expr = erhs.Expr;
                    }
                }
            }

            return vds;
        }

        protected override UpdateStmt Visit(UpdateStmt up)
        {
            for (int i = 0; i < up.Lhss.Count; i++)
            {
                if (up.Lhss[i] is NameSegment nm && nm.Name == InlineVar.Name && curTable.Lookup(nm.Name).GetHashCode() == InlineVar.tableDeclaration.GetHashCode())
                {
                    if (InlineVar.expr == null && up.Rhss[i] is ExprRhs erhs)
                    {
                        InlineVar.expr = erhs.Expr;
                    }
                    else
                    {
                        InlineVar.isUpdated = true;
                    }
                }
            }

            return up;
        }
    }
}

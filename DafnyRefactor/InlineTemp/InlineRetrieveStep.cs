namespace Microsoft.Dafny
{
    public class InlineRetrieveStep : DafnyVisitor
    {
        protected SymbolTable curTable;
        public SymbolTableDeclaration declaration { protected get; set; }
        public SymbolTable table { protected get; set; }
        public InlineVar inlineVar { get; protected set; }

        public override void execute()
        { 
            curTable = table;
            inlineVar = new InlineVar();
            inlineVar.tableDeclaration = declaration;

            base.execute();
        }

        protected override WhileStmt next(WhileStmt while_)
        {
            curTable = curTable.lookupTable(while_.Tok.GetHashCode());

            foreach (Statement stmt in while_.Body.Body)
            {
                next(stmt);
            }

            curTable = curTable.parent;

            return while_;
        }

        protected override VarDeclStmt next(VarDeclStmt vds)
        {
            if (vds.Update is UpdateStmt up)
            {
                for (int i = 0; i < up.Lhss.Count; i++)
                {
                    if (up.Lhss[i] is AutoGhostIdentifierExpr agie && agie.Name == inlineVar.name && curTable.lookup(agie.Name).GetHashCode() == inlineVar.tableDeclaration.GetHashCode())
                    {
                        ExprRhs erhs = (ExprRhs)up.Rhss[i];
                        inlineVar.expr = erhs.Expr;
                    }
                }
            }

            return vds;
        }

        protected override UpdateStmt next(UpdateStmt up)
        {
            for (int i = 0; i < up.Lhss.Count; i++)
            {
                if (up.Lhss[i] is NameSegment nm && nm.Name == inlineVar.name && curTable.lookup(nm.Name).GetHashCode() == inlineVar.tableDeclaration.GetHashCode())
                {
                    if (inlineVar.expr == null && up.Rhss[i] is ExprRhs erhs)
                    {
                        inlineVar.expr = erhs.Expr;
                    }
                    else
                    {
                        inlineVar.isUpdated = true;
                    }
                }
            }

            return up;
        }
    }
}

namespace Microsoft.Dafny
{
    public class InlineRefactorStep : DafnyVisitor
    {
        protected SymbolTable curTable;
        public SymbolTable table;
        public InlineVar inlineVar;

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

        public override void execute()
        {
            curTable = table;
            base.execute();
        }

        protected override UpdateStmt next(UpdateStmt up)
        {
            for (int i = 0; i < up.Rhss.Count; i++)
            {
                if (up.Rhss[i] is ExprRhs erhs && erhs.Expr != null)
                {
                    up.Rhss[i] = new ExprRhs(applyInlineTemp(erhs.Expr));
                }
            }

            return up;
        }

        protected override AssertStmt next(AssertStmt assert)
        {
            var expr = applyInlineTemp(assert.Expr);
            var newAssert = new AssertStmt(assert.Tok, assert.EndTok, expr, assert.Proof, assert.Label, assert.Attributes);
            return newAssert;
        }

        protected Expression applyInlineTemp(Expression exp)
        {
            var outExp = exp;

            if (outExp is NameSegment nameSeg && nameSeg.Name == inlineVar.name && curTable.lookup(nameSeg.Name).GetHashCode() == inlineVar.tableDeclaration.GetHashCode())
            {
                outExp = inlineVar.expr;
            }
            else if (outExp is BinaryExpr subExp)
            {
                var e0 = applyInlineTemp(subExp.E0);
                var e1 = applyInlineTemp(subExp.E1);
                outExp = new BinaryExpr(subExp.tok, subExp.Op, e0, e1);
            }

            return outExp;
        }
    }
}

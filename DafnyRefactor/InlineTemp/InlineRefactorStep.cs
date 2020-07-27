namespace Microsoft.Dafny
{
    public class InlineRefactorStep : DafnyWithTableVisitor
    {
        protected InlineVariable inlineVar;

        public InlineRefactorStep(Program program, SymbolTable rootTable, InlineVariable inlineVar) : base(program, rootTable)
        {
            this.inlineVar = inlineVar;
        }

        public override void Execute()
        {
            curTable = rootTable;
            base.Execute();
        }

        protected override UpdateStmt Visit(UpdateStmt up)
        {
            for (int i = 0; i < up.Rhss.Count; i++)
            {
                if (up.Rhss[i] is ExprRhs erhs && erhs.Expr != null)
                {
                    up.Rhss[i] = new ExprRhs(ApplyInlineTemp(erhs.Expr));
                }
            }

            return up;
        }

        protected override AssertStmt Visit(AssertStmt assert)
        {
            var expr = ApplyInlineTemp(assert.Expr);
            var newAssert = new AssertStmt(assert.Tok, assert.EndTok, expr, assert.Proof, assert.Label, assert.Attributes);
            return newAssert;
        }

        protected Expression ApplyInlineTemp(Expression exp)
        {
            var outExp = exp;

            if (outExp is NameSegment nameSeg && nameSeg.Name == inlineVar.Name && curTable.Lookup(nameSeg.Name).GetHashCode() == inlineVar.tableDeclaration.GetHashCode())
            {
                outExp = inlineVar.expr;
            }
            else if (outExp is BinaryExpr subExp)
            {
                var e0 = ApplyInlineTemp(subExp.E0);
                var e1 = ApplyInlineTemp(subExp.E1);
                outExp = new BinaryExpr(subExp.tok, subExp.Op, e0, e1);
            }

            return outExp;
        }
    }
}

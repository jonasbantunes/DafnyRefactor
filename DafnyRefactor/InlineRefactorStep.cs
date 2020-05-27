namespace Microsoft.Dafny
{
    public class InlineRefactorStep : RefatorStepBase
    {
        public InlineVar inlineVar { get; }
        public InlineRefactorStep(Program program, InlineVar inlineVar) : base(program)
        {
            this.inlineVar = inlineVar;
        }

        protected override void next(UpdateStmt up)
        {
            for (int i = 0; i < up.Rhss.Count; i++)
            {
                if (up.Rhss[i] is ExprRhs erhs && erhs.Expr != null)
                {
                    up.Rhss[i] = new ExprRhs(applyInlineTemp(erhs.Expr));
                }
            }
        }

        protected Expression applyInlineTemp(Expression exp)
        {
            var outExp = exp;

            if (outExp is NameSegment nameSeg && nameSeg.Name == inlineVar.name)
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

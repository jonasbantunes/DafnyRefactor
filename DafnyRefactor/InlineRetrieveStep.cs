namespace Microsoft.Dafny
{
    public class InlineRetrieveStep: RefatorStepBase
    {
        public InlineVar inlineVar { get; }

        public InlineRetrieveStep(Program program, string method, string name): base(program)
        {
            inlineVar = new InlineVar();
            inlineVar.method = method;
            inlineVar.name = name;
        }

        protected override void next(VarDeclStmt vds)
        {
            if (vds.Update is UpdateStmt up)
            {
                for (int i = 0; i < up.Lhss.Count; i++)
                {
                    if (up.Lhss[i] is AutoGhostIdentifierExpr agie && agie.Name == inlineVar.name)
                    {
                        ExprRhs erhs = (ExprRhs)up.Rhss[i];
                        inlineVar.expr = erhs.Expr;
                    }
                }
            }
        }

        protected override void next(UpdateStmt up)
        {
            for (int i = 0; i < up.Lhss.Count; i++)
            {
                if (up.Lhss[i] is NameSegment nm && nm.Name == inlineVar.name)
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
        }
    }
}

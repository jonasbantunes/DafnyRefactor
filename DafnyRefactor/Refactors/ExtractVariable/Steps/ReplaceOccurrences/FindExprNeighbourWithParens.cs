using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.ExtractVariable
{
    public class FindExprNeighbourWithParens : FindExprNeighbours
    {
        public FindExprNeighbourWithParens(Expression rootExpr, int position) : base(rootExpr, position)
        {
        }

        protected override void Visit(ParensExpression parensExpression)
        {
            VerifyExpr(parensExpression);
            base.Visit(parensExpression);
        }
    }
}
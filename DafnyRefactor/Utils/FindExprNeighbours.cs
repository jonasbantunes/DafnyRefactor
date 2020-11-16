using Microsoft.Dafny;

namespace DafnyRefactor.Utils
{
    /// <summary>
    ///     Finds the leftmost and rightmost expressions of a informed expression on AST>
    /// </summary>
    public class FindExprNeighbours : DafnyVisitor
    {
        protected int position;
        protected Expression rootExpr;
        protected Statement rootStmt;

        public FindExprNeighbours(Statement rootStmt, int position)
        {
            this.rootStmt = rootStmt;
            this.position = position;
        }

        public FindExprNeighbours(Expression rootExpr, int position)
        {
            this.rootExpr = rootExpr;
            this.position = position;
        }

        public Expression LeftExpr { get; protected set; }
        public Expression RightExpr { get; protected set; }

        public void Execute()
        {
            LeftExpr = null;
            RightExpr = null;

            if (rootStmt != null)
            {
                Visit(rootStmt);
            }
            else
            {
                Visit(rootExpr);
            }
        }

        protected override void Visit(Expression exp)
        {
            if (exp is NameSegment || exp is ExprDotName || exp is LiteralExpr)
            {
                VerifyExpr(exp);
                return;
            }

            base.Visit(exp);
        }

        protected override void Visit(BinaryExpr binaryExpr)
        {
            Visit(binaryExpr.E0);

            if (binaryExpr.tok.pos < position)
            {
                LeftExpr = binaryExpr;
            }
            else if (RightExpr == null)
            {
                RightExpr = binaryExpr;
            }

            Visit(binaryExpr.E1);
        }

        protected override void Visit(NegationExpression negationExpr)
        {
            if (negationExpr.tok.pos < position)
            {
                LeftExpr = negationExpr;
            }
            else if (RightExpr == null)
            {
                RightExpr = negationExpr;
            }

            Visit(negationExpr.E);
        }

        protected override void Visit(ITEExpr iteExpr)
        {
            VerifyExpr(iteExpr);
            base.Visit(iteExpr);
        }

        protected override void Visit(ThisExpr thisExpr)
        {
            if (!thisExpr.IsImplicit)
            {
                VerifyExpr(thisExpr);
            }

            base.Visit(thisExpr);
        }

        protected void VerifyExpr(Expression expr)
        {
            var endTokPos = expr.tok.pos + expr.tok.val.Length - 1;
            if (endTokPos < position)
            {
                LeftExpr = expr;
            }
            else if (RightExpr == null)
            {
                RightExpr = expr;
            }
        }
    }
}
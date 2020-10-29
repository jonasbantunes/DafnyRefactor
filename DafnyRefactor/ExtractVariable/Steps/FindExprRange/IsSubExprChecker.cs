using System;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public class IsSubExprChecker : DafnyVisitor
    {
        protected readonly Expression rootExpr;
        protected readonly Expression subExpr;
        protected bool isSubExpr;

        protected IsSubExprChecker(Expression subExpr, Expression rootExpr)
        {
            if (subExpr == null || rootExpr == null) throw new ArgumentNullException();

            this.subExpr = subExpr;
            this.rootExpr = rootExpr;
        }

        protected void Execute()
        {
            isSubExpr = false;
            Visit(rootExpr);
        }

        protected override void Visit(Expression exp)
        {
            if (exp == subExpr)
            {
                isSubExpr = true;
                return;
            }

            base.Visit(exp);
        }

        public static bool IsSubExpr(Expression subExpr, Expression rootExpr)
        {
            var checker = new IsSubExprChecker(subExpr, rootExpr);
            checker.Execute();
            return checker.isSubExpr;
        }
    }
}
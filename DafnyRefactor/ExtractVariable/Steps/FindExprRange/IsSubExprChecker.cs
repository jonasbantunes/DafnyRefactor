using System;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.ExtractVariable
{
    public class IsSubExprChecker : DafnyVisitor
    {
        private readonly Expression _rootExpr;
        private readonly Expression _subExpr;
        private bool _isSubExpr;

        private IsSubExprChecker(Expression subExpr, Expression rootExpr)
        {
            if (subExpr == null || rootExpr == null) throw new ArgumentNullException();

            _subExpr = subExpr;
            _rootExpr = rootExpr;
        }

        private void Execute()
        {
            _isSubExpr = false;
            Visit(_rootExpr);
        }

        protected override void Visit(Expression exp)
        {
            if (exp == _subExpr)
            {
                _isSubExpr = true;
                return;
            }

            base.Visit(exp);
        }

        public static bool IsSubExpr(Expression subExpr, Expression rootExpr)
        {
            var checker = new IsSubExprChecker(subExpr, rootExpr);
            checker.Execute();
            return checker._isSubExpr;
        }
    }
}
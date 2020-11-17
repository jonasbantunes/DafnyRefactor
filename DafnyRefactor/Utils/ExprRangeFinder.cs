using System;
using DafnyRefactor.ExtractVariable;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils
{
    public class ExprRangeFinder
    {
        private readonly Expression _exp;
        private readonly string _sourceCode;
        private Range _exprRange;

        private ExprRangeFinder(Expression exp, string sourceCode)
        {
            if (exp == null || sourceCode == null) throw new ArgumentNullException();

            _exp = exp;
            _sourceCode = sourceCode;
        }

        private void Execute()
        {
            _exprRange = null;

            var startFinder = new FindExprNeighbourWithParens(_exp, 0);
            startFinder.Execute();
            var startExpr = startFinder.RightExpr;

            var endFinder = new FindExprNeighbourWithParens(_exp, int.MaxValue);
            endFinder.Execute();
            var endExpr = endFinder.LeftExpr;

            if (startExpr == null || endExpr == null) return;


            int startPos;
            if (startExpr is ExprDotName exprDotName)
            {
                var lhs = exprDotName.Lhs;
                while (lhs is ExprDotName subDotExpr)
                {
                    lhs = subDotExpr.Lhs;
                }

                startPos = lhs.tok.pos;
            }
            else
            {
                startPos = startExpr.tok.pos;
            }

            var endPos = FindRealEnd(startPos, endExpr.tok.pos + endExpr.tok.val.Length);
            _exprRange = new Range(startPos, endPos);
        }

        private int FindRealEnd(int realStart, int end)
        {
            var openedParens = 0;
            var i = realStart;
            while (i < end || openedParens > 0)
            {
                var c = _sourceCode[i];

                switch (c)
                {
                    case '(':
                        openedParens++;
                        break;
                    case ')':
                        openedParens--;
                        break;
                }

                i++;
            }

            return i;
        }

        public static Range Find(Expression exp, string sourceCode)
        {
            var finder = new ExprRangeFinder(exp, sourceCode);
            finder.Execute();
            return finder._exprRange;
        }
    }
}
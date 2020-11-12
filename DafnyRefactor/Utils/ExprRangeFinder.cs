using System;
using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.ExtractVariable;

namespace Microsoft.DafnyRefactor.Utils
{
    public class ExprRangeFinder
    {
        protected Expression exp;
        protected Range exprRange;
        protected string sourceCode;

        protected ExprRangeFinder(Expression exp, string sourceCode)
        {
            if (exp == null || sourceCode == null) throw new ArgumentNullException();

            this.exp = exp;
            this.sourceCode = sourceCode;
        }

        protected void Execute()
        {
            exprRange = null;

            var startFinder = new FindExprNeighbourWithParens(exp, 0);
            startFinder.Execute();
            var startExpr = startFinder.RightExpr;

            var endFinder = new FindExprNeighbourWithParens(exp, int.MaxValue);
            endFinder.Execute();
            var endExpr = endFinder.LeftExpr;

            if (startExpr == null || endExpr == null) return;


            int startPos;
            if (startExpr is ExprDotName exprDotName)
            {
                var lhs = exprDotName.Lhs;
                while (!(lhs is NameSegment))
                {
                    var subDotExpr = (ExprDotName) lhs;
                    lhs = subDotExpr.Lhs;
                }

                startPos = lhs.tok.pos;
            }
            else
            {
                startPos = startExpr.tok.pos;
            }

            var endPos = FindRealEnd(startPos, endExpr.tok.pos + endExpr.tok.val.Length);
            exprRange = new Range(startPos, endPos);
        }

        protected int FindRealEnd(int realStart, int end)
        {
            var openedParens = 0;
            var i = realStart;
            while (i < end || openedParens > 0)
            {
                var c = sourceCode[i];

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

        protected List<Range> SubExprRawRanges(string rawExpr, string rawSub)
        {
            var ranges = new List<Range>();

            var i = 0;
            while (i < rawExpr.Length)
            {
                var range = SubExprRawRange(rawExpr, rawSub, i);
                if (range == null) break;
                ranges.Add(range);
                i = range.end;
            }

            return ranges;
        }

        protected Range SubExprRawRange(string rawExpr, string rawSub, int offset)
        {
            var range = rawExpr.IndexOfWithIgnores(rawSub, offset);
            if (range == null) return null;
            if (rawExpr[range.start] == '-' && !StartsWithUnary(rawExpr, range.start)) return null;
            return range;
        }

        protected bool StartsWithUnary(string rawExpr, int exprStart)
        {
            if (rawExpr[exprStart] != '-') return false;

            var i = exprStart - 1;
            while (0 <= i && i < rawExpr.Length)
            {
                var c = rawExpr[i];
                if (char.IsLetterOrDigit(c))
                    return false;
                if (c == '+' || c == '-' || c == '*' || c == '/' || c == '(' || c == ')') return true;

                i--;
            }

            return true;
        }

        public static Range Find(Expression exp, string sourceCode)
        {
            var finder = new ExprRangeFinder(exp, sourceCode);
            finder.Execute();
            return finder.exprRange;
        }
    }
}
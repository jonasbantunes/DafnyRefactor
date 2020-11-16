using System;
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.ExtractVariable
{
    // TOOD: Replace some methods with ExprRangeFinder.
    public class ExprOcurrencesReplacer : DafnyVisitorWithNearests
    {
        protected List<SourceEdit> assertSourceEdits;
        protected Range exprRange;
        protected Statement extractStmt;
        protected Program program;
        protected string rawProgram;
        protected IExtractVariableScope rootScope;
        protected List<SourceEdit> sourceEdits;
        protected List<int> stmtDivisors;
        protected List<IRefactorVariable> variables;
        protected string varName;

        protected ExprOcurrencesReplacer(Program program, string rawProgram, Range exprRange, string varName,
            List<int> stmtDivisors, Statement extractStmt, IExtractVariableScope rootScope,
            List<IRefactorVariable> variables)
        {
            if (program == null || rawProgram == null || exprRange == null || varName == null || rootScope == null ||
                variables == null)
                throw new ArgumentNullException();

            this.program = program;
            this.rawProgram = rawProgram;
            this.exprRange = exprRange;
            this.varName = varName;
            this.stmtDivisors = stmtDivisors;
            this.extractStmt = extractStmt;
            this.rootScope = rootScope;
            this.variables = variables;
        }

        //protected string VarName => $"({varName})";
        protected string VarName => varName;

        protected void Execute()
        {
            sourceEdits = new List<SourceEdit>();
            assertSourceEdits = new List<SourceEdit>();

            Visit(program);
        }

        protected override void Visit(Expression exp)
        {
            var curScope = rootScope.EvrFindScope(nearestScopeToken.GetHashCode());
            if (curScope == null) return;
            if (!curScope.IsReplacable()) return;
            if (extractStmt.Tok.pos > exp.tok.pos) return;
            if (exp is AutoGhostIdentifierExpr) return;

            var range = FindExprRange(exp);
            if (range.start == 0 && range.end == 0) return;

            var expRaw = rawProgram.Substring(range.start, range.end - range.start);
            var varRaw = rawProgram.Substring(exprRange.start, exprRange.end - exprRange.start).Trim();

            var ranges = SubExprRawRanges(expRaw, varRaw);
            if (ranges.Count == 0) return;

            var replacedRaw = expRaw;
            for (var i = ranges.Count - 1; i >= 0; i--)
            {
                var subRange = ranges[i];
                var offsettedRange = new Range(range.start + subRange.start, range.start + subRange.end);
                if (!ExprIsReplaceableChecker.IsReplacable(program, offsettedRange, rootScope, variables))
                    continue;

                replacedRaw = replacedRaw.Remove(subRange.start, subRange.end - subRange.start)
                    .Insert(subRange.start, VarName);
            }

            if (replacedRaw.Equals(expRaw)) return;
            sourceEdits.Add(new SourceEdit(range.start, range.end, replacedRaw));

            var assert = $"\n assert {varName} == ( {varRaw} );";
            var divisorIndex = stmtDivisors.FindIndex(divisor => divisor >= exp.tok.pos);
            if (divisorIndex < 1) return;
            var assertPos = stmtDivisors[divisorIndex - 1] + 1;
            assertSourceEdits.Add(new SourceEdit(assertPos, assert));
        }

        protected Range FindExprRange(Expression exp)
        {
            var startFinder = new FindExprNeighbourWithParens(exp, 0);
            startFinder.Execute();
            var startExpr = startFinder.RightExpr;

            var endFinder = new FindExprNeighbourWithParens(exp, int.MaxValue);
            endFinder.Execute();
            var endExpr = endFinder.LeftExpr;

            if (startExpr == null || endExpr == null) return new Range(0, 0);

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
            return new Range(startPos, endPos);
        }

        protected int FindRealEnd(int realStart, int end)
        {
            var openedParens = 0;
            var i = realStart;
            while (i < end || openedParens > 0)
            {
                var c = rawProgram[i];

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
            //var start = rawExpr.IndexOf(rawSub, offset, StringComparison.Ordinal);
            //if (start == -1) return null;
            //if (rawExpr[start] == '-' && !StartsWithUnary(rawExpr, start)) return null;
            //return new Range(start, start + rawSub.Length);

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

        public static (List<SourceEdit> edits, List<SourceEdit> asserts) Replace(Program program, string rawProgram,
            Range exprRange, string varName,
            List<int> stmtDivisors, Statement extractStmt, IExtractVariableScope rootScope,
            List<IRefactorVariable> variables)
        {
            var replacer = new ExprOcurrencesReplacer(program, rawProgram, exprRange, varName, stmtDivisors,
                extractStmt, rootScope, variables);
            replacer.Execute();
            return (replacer.sourceEdits, replacer.assertSourceEdits);
        }
    }

    public static class IndexOfWithIgnoresExtension
    {
        public static Range IndexOfWithIgnores(this string str, string sub, int offset)
        {
            var strStart = offset;
            while (strStart < str.Length && str[strStart] == ' ')
            {
                strStart++;
            }

            if (strStart >= str.Length) return null;

            var subStart = 0;
            while (subStart < sub.Length && sub[subStart] == ' ')
            {
                subStart++;
            }

            if (subStart >= sub.Length) return null;

            var i = strStart;
            while (i < str.Length)
            {
                if (str[i] == sub[subStart])
                {
                    var strPos = i;
                    var subPos = subStart;

                    while (strPos < str.Length && subPos < sub.Length)
                    {
                        if (sub[subPos] == ' ')
                        {
                            subPos++;
                            continue;
                        }

                        if (str[strPos] == ' ')
                        {
                            strPos++;
                            continue;
                        }

                        if (str[strPos] != sub[subPos]) break;

                        strPos++;
                        subPos++;
                    }

                    if (subPos >= sub.Length)
                    {
                        return new Range(i, strPos);
                    }
                }

                i++;
            }

            return null;
        }
    }
}
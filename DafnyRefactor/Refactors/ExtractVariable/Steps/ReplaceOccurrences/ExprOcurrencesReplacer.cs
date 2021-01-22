using System;
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.ExtractVariable
{
    // TOOD: Replace some methods with ExprRangeFinder.
    public class ExprOcurrencesReplacer : DafnyVisitorWithNearests
    {
        private readonly Range _exprRange;
        private readonly Statement _extractStmt;
        private readonly Program _program;
        private readonly string _rawProgram;
        private readonly IExtractVariableScope _rootScope;
        private readonly List<int> _stmtDivisors;
        private readonly List<IRefactorVariable> _variables;
        private readonly string _varName;
        private List<SourceEdit> _assertSourceEdits;
        private List<SourceEdit> _sourceEdits;

        private ExprOcurrencesReplacer(Program program, string rawProgram, Range exprRange, string varName,
            List<int> stmtDivisors, Statement extractStmt, IExtractVariableScope rootScope,
            List<IRefactorVariable> variables)
        {
            if (program == null || rawProgram == null || exprRange == null || varName == null || rootScope == null ||
                variables == null)
                throw new ArgumentNullException();

            _program = program;
            _rawProgram = rawProgram;
            _exprRange = exprRange;
            _varName = varName;
            _stmtDivisors = stmtDivisors;
            _extractStmt = extractStmt;
            _rootScope = rootScope;
            _variables = variables;
        }

        private void Execute()
        {
            _sourceEdits = new List<SourceEdit>();
            _assertSourceEdits = new List<SourceEdit>();

            Visit(_program);
        }

        protected override void Visit(Expression exp)
        {
            var curScope = _rootScope.EvrFindScope(nearestScopeToken.GetHashCode());
            if (curScope == null) return;
            if (!curScope.IsReplacable()) return;
            if (_extractStmt.Tok.pos > exp.tok.pos) return;
            if (exp is AutoGhostIdentifierExpr) return;

            var range = FindExprRange(exp);
            if (range.start == 0 && range.end == 0) return;

            var expRaw = _rawProgram.Substring(range.start, range.end - range.start);
            var varRaw = _rawProgram.Substring(_exprRange.start, _exprRange.end - _exprRange.start).Trim();

            var ranges = SubExprRawRanges(expRaw, varRaw);
            if (ranges.Count == 0) return;

            var replacedRaw = expRaw;
            for (var i = ranges.Count - 1; i >= 0; i--)
            {
                var subRange = ranges[i];
                var offsettedRange = new Range(range.start + subRange.start, range.start + subRange.end);
                if (!ExprIsReplaceableChecker.IsReplacable(_program, offsettedRange, _rootScope, _variables))
                    continue;

                replacedRaw = replacedRaw.Remove(subRange.start, subRange.end - subRange.start)
                    .Insert(subRange.start, _varName);
            }

            if (replacedRaw.Equals(expRaw)) return;
            _sourceEdits.Add(new SourceEdit(range.start, range.end, replacedRaw));

            var assert = $"{Environment.NewLine} assert {_varName} == ( {varRaw} );";
            var divisorIndex = _stmtDivisors.FindIndex(divisor => divisor >= exp.tok.pos);
            if (divisorIndex < 1) return;
            var assertPos = _stmtDivisors[divisorIndex - 1] + 1;
            _assertSourceEdits.Add(new SourceEdit(assertPos, assert));
        }

        private Range FindExprRange(Expression exp)
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

        private int FindRealEnd(int realStart, int end)
        {
            var openedParens = 0;
            var i = realStart;
            while (i < end || openedParens > 0)
            {
                var c = _rawProgram[i];

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

        private List<Range> SubExprRawRanges(string rawExpr, string rawSub)
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

        private Range SubExprRawRange(string rawExpr, string rawSub, int offset)
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

        private bool StartsWithUnary(string rawExpr, int exprStart)
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
            return (replacer._sourceEdits, replacer._assertSourceEdits);
        }
    }
}
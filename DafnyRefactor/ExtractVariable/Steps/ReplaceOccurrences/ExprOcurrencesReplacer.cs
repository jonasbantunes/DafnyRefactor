using System;
using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public class ExprOcurrencesReplacer : DafnyVisitor
    {
        protected List<SourceEdit> assertSourceEdits;
        protected Range exprRange;
        protected Statement extractStmt;
        protected Expression furtherstExpr;
        protected Program program;
        protected string rawProgram;
        protected IRefactorScope rootScope;
        protected List<SourceEdit> sourceEdits;
        protected List<int> stmtDivisors;
        protected List<IRefactorVariable> variables;
        protected string varName;

        protected ExprOcurrencesReplacer(Program program, string rawProgram, Range exprRange, string varName,
            List<int> stmtDivisors, Statement extractStmt, IRefactorScope rootScope, List<IRefactorVariable> variables)
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
            furtherstExpr = null;
            sourceEdits = new List<SourceEdit>();
            assertSourceEdits = new List<SourceEdit>();

            AddDeclAssertive();
            Visit(program);
        }

        protected void AddDeclAssertive()
        {
            var divisorIndex = stmtDivisors.FindIndex(divisor => divisor >= extractStmt.Tok.pos);
            if (divisorIndex < 1) return;
            var ghostPos = stmtDivisors[divisorIndex - 1] + 1;

            var varRaw = rawProgram.Substring(exprRange.start, exprRange.end - exprRange.start);
            var ghostRaw = $"\n ghost var {varName}___RefactorGhostExpr := {varRaw};";
            assertSourceEdits.Add(new SourceEdit(ghostPos, ghostRaw));
        }

        protected override void Visit(Expression exp)
        {
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

            var assert = $"\n assert ({varName}___RefactorGhostExpr) == ( {varRaw} );";
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
            var start = rawExpr.IndexOf(rawSub, offset, StringComparison.Ordinal);
            if (start == -1) return null;
            return new Range(start, start + rawSub.Length);
        }

        public static (List<SourceEdit> edits, List<SourceEdit> asserts) Replace(Program program, string rawProgram,
            Range exprRange, string varName,
            List<int> stmtDivisors, Statement extractStmt, IRefactorScope rootScope, List<IRefactorVariable> variables)
        {
            var replacer = new ExprOcurrencesReplacer(program, rawProgram, exprRange, varName, stmtDivisors,
                extractStmt, rootScope, variables);
            replacer.Execute();
            return (replacer.sourceEdits, replacer.assertSourceEdits);
        }
    }
}
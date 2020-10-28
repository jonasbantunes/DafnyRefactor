using System;
using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    /// <summary>
    ///     A <c>RefactorStep</c> that replaces all occurences of extracted expression.
    ///     <para>Currently is replacing only the original selection.</para>
    /// </summary>
    public class ReplaceOccurrencesStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        protected TState inState;
        protected ReplaceOcurrencesVisitor replacer;
        protected EditsValidator validator;

        public override void Handle(TState state)
        {
            if (state == null || state.EvExprRange == null || state.EvStmt == null || state.Program == null ||
                state.EvSourceCode == null || state.EvOptions == null || state.StmtDivisors == null ||
                state.SourceEdits == null || state.EvRootScope == null)
                throw new ArgumentNullException();

            inState = state;

            Replace();
            Validate();
            if (state.Errors.Count > 0) return;

            state.SourceEdits.AddRange(replacer.SourceEdits);
            base.Handle(state);
        }

        protected void Replace()
        {
            replacer = new ReplaceOcurrencesVisitor(inState.Program, inState.EvSourceCode, inState.EvExprRange,
                inState.EvOptions.VarName, inState.StmtDivisors, inState.EvStmt, inState.EvRootScope,
                inState.EvExprVariables);
            replacer.Execute();
        }

        protected void Validate()
        {
            var edits = new List<SourceEdit>();
            edits.AddRange(inState.SourceEdits);
            edits.AddRange(replacer.AssertSourceEdits);
            validator = new EditsValidator(inState.FilePath, edits);
            validator.Execute();

            if (!validator.IsValid) inState.Errors.Add("Error: Invalid selection");
        }
    }

    public class ReplaceOcurrencesVisitor : DafnyVisitor
    {
        protected Range exprRange;
        protected Expression furtherstExpr;
        protected Program program;
        protected string rawProgram;
        protected List<int> stmtDivisors;
        protected string varName;
        protected Statement extractStmt;
        protected IEvScope rootScope;
        protected List<IRefactorVariable> variables;

        public ReplaceOcurrencesVisitor(Program program, string rawProgram, Range exprRange, string varName,
            List<int> stmtDivisors, Statement extractStmt, IEvScope rootScope, List<IRefactorVariable> variables)
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
        public List<SourceEdit> SourceEdits { get; protected set; }
        public List<SourceEdit> AssertSourceEdits { get; protected set; }

        public void Execute()
        {
            furtherstExpr = null;
            SourceEdits = new List<SourceEdit>();
            AssertSourceEdits = new List<SourceEdit>();

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
            AssertSourceEdits.Add(new SourceEdit(ghostPos, ghostRaw));
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
                if (!ExpIsReplaceable(new Range(range.start + subRange.start, range.start + subRange.end)))
                    continue;

                replacedRaw = replacedRaw.Remove(subRange.start, subRange.end - subRange.start)
                    .Insert(subRange.start, VarName);
            }

            if (replacedRaw.Equals(expRaw)) return;
            SourceEdits.Add(new SourceEdit(range.start, range.end, replacedRaw));

            var assert = $"\n assert ({varName}___RefactorGhostExpr) == ( {varRaw} );";
            var divisorIndex = stmtDivisors.FindIndex(divisor => divisor >= exp.tok.pos);
            if (divisorIndex < 1) return;
            var assertPos = stmtDivisors[divisorIndex - 1] + 1;
            AssertSourceEdits.Add(new SourceEdit(assertPos, assert));
        }

        public Range FindExprRange(Expression exp)
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

        public int FindRealEnd(int realStart, int end)
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

        public List<Range> SubExprRawRanges(string rawExpr, string rawSub)
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

        public Range SubExprRawRange(string rawExpr, string rawSub, int offset)
        {
            var start = rawExpr.IndexOf(rawSub, offset, StringComparison.Ordinal);
            if (start == -1) return null;
            return new Range(start, start + rawSub.Length);
        }

        // TODO: move this to static method
        protected bool ExpIsReplaceable(Range subRange)
        {
            var checker = new EvExpIsReplaceable(program, subRange, rootScope, variables);
            checker.Execute();
            return checker.IsReplaceable;
        }
    }

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

    internal class EvExpIsReplaceable : DafnyVisitorWithNearests
    {
        protected Program program;
        protected Range expRange;
        protected IEvScope rootScope;
        protected List<IRefactorVariable> variables;
        public bool IsReplaceable { get; protected set; }

        public EvExpIsReplaceable(Program program, Range expRange, IEvScope rootScope,
            List<IRefactorVariable> variables)
        {
            this.program = program;
            this.expRange = expRange;
            this.rootScope = rootScope;
            this.variables = variables;
        }

        public void Execute()
        {
            IsReplaceable = true;
            Visit(program);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            if (expRange.start > nameSeg.tok.pos || nameSeg.tok.pos > expRange.end) return;

            var curScope = rootScope.FindScope(nearestScopeToken.GetHashCode());
            if (curScope == null) return;

            var variable = curScope.LookupVariable(nameSeg.Name);
            if (variable == null) return;

            var found = variables.FindIndex(v => v.Equals(variable));
            if (found == -1)
            {
                IsReplaceable = false;
                return;
            }

            base.Visit(nameSeg);
        }
    }
}
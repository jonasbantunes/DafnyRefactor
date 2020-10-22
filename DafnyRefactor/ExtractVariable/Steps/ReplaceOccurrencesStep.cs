using System;
using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public class ReplaceOccurrencesStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        protected TState inState;
        protected ReplaceOcurrencesVisitor replacer;
        protected EditsValidator validator;

        public override void Handle(TState state)
        {
            if (state == null || state.ExprRange == null || state.Program == null || state.RawProgram == null ||
                state.ExtractVariableOptions == null || state.StmtDivisors == null || state.SourceEdits == null)
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
            replacer = new ReplaceOcurrencesVisitor(inState.Program, inState.RawProgram, inState.ExprRange,
                inState.ExtractVariableOptions.VarName, inState.StmtDivisors);
            replacer.Execute();
        }

        protected void Validate()
        {
            var edits = new List<SourceEdit>();
            edits.AddRange(inState.SourceEdits);
            edits.AddRange(replacer.SourceEdits);
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

        public ReplaceOcurrencesVisitor(Program program, string rawProgram, Range exprRange, string varName,
            List<int> stmtDivisors)
        {
            if (program == null || rawProgram == null || exprRange == null || varName == null)
                throw new ArgumentNullException();

            this.program = program;
            this.rawProgram = rawProgram;
            this.exprRange = exprRange;
            this.varName = varName;
            this.stmtDivisors = stmtDivisors;
        }

        public List<SourceEdit> SourceEdits { get; protected set; }
        public List<SourceEdit> AssertSourceEdits { get; protected set; }

        public void Execute()
        {
            furtherstExpr = null;
            SourceEdits = new List<SourceEdit>();
            AssertSourceEdits = new List<SourceEdit>();

            Visit(program);
        }

        protected override void Visit(Expression exp)
        {
            /* FIND START AND END OF EXPRESSION */
            var startFinder = new FindExprNeighbours(exp, 0);
            startFinder.Execute();
            var startExpr = startFinder.RightExpr;

            var endFinder = new FindExprNeighbours(exp, int.MaxValue);
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

            var endPos = endExpr.tok.pos + endExpr.tok.val.Length;
            if (startPos >= endPos) return;
            if (exprRange.end < startPos || exprRange.start > endPos) return;

            /* ASSERTIVE */
            var rawExpr = rawProgram.Substring(startPos, endPos - startPos);

            var replaceStart = exprRange.start >= startPos ? exprRange.start - startPos : 0;
            var replaceEnd = exprRange.end <= endPos ? exprRange.end - startPos : rawExpr.Length;
            var replacedRawExpr = rawExpr.Remove(replaceStart, replaceEnd - replaceStart)
                .Insert(replaceStart, varName);
            var assert = $"\n assert {replacedRawExpr} == {rawExpr};";

            var divisorIndex = stmtDivisors.FindIndex(divisor => divisor >= exp.tok.pos);
            if (divisorIndex < 1) return;
            var assertPos = stmtDivisors[divisorIndex - 1] + 1;

            /* SAVE EDITS */
            SourceEdits.Add(new SourceEdit(exprRange.start, exprRange.end, varName));
            AssertSourceEdits.Add(new SourceEdit(assertPos, assert));
        }
    }
}
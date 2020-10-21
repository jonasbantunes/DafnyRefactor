using System;
using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public class ReplaceOccurrencesStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.ExprRange == null || state.Program == null || state.RawProgram == null ||
                state.ExtractVariableOptions == null || state.StmtDivisors == null || state.SourceEdits == null)
                throw new ArgumentNullException();

            var replacer = new ReplaceOcurrencesVisitor(state.Program, state.RawProgram, state.ExprRange,
                state.ExtractVariableOptions.VarName, state.StmtDivisors);
            replacer.Execute();

            var replacerEdits = new List<SourceEdit>();
            replacerEdits.AddRange(state.SourceEdits);
            replacerEdits.AddRange(replacer.SourceEdits);
            replacerEdits.AddRange(replacer.AssertSourceEdits);
            var validator = new EditsValidator(state.FilePath, replacerEdits);
            validator.Execute();
            if (!validator.IsValid)
            {
                state.Errors.Add("Error: Invalid selection");
                return;
            }

            state.SourceEdits.AddRange(replacer.SourceEdits);

            base.Handle(state);
        }
    }

    internal class ReplaceOcurrencesVisitor : DafnyVisitor
    {
        protected Program program;
        protected string rawProgram;
        protected Range exprRange;
        protected string exprString;
        protected Expression furtherstExpr;
        protected string varName;
        protected List<int> stmtDivisors;
        public List<SourceEdit> SourceEdits { get; protected set; }
        public List<SourceEdit> AssertSourceEdits { get; protected set; }

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

        public void Execute()
        {
            var startPos = exprRange.start;
            var endPos = exprRange.end;
            exprString = rawProgram.Substring(startPos, endPos - startPos);

            furtherstExpr = null;
            SourceEdits = new List<SourceEdit>();
            AssertSourceEdits = new List<SourceEdit>();
            Visit(program);
        }

        protected override void Visit(Expression exp)
        {
            var startFinder = new FindExprVisitor(exp, 0);
            startFinder.Execute();
            var startExpr = startFinder.RightExpr;

            var endFinder = new FindExprVisitor(exp, int.MaxValue);
            endFinder.Execute();
            var endExpr = endFinder.LeftExpr;

            if (startExpr == null || endExpr == null) return;

            var startPos = startExpr.tok.pos;
            var endPos = endExpr.tok.pos + endExpr.tok.val.Length;
            if (startPos >= endPos) return;
            if (startPos > exprRange.start || exprRange.end > endPos) return;

            SourceEdits.Add(new SourceEdit(exprRange.start, exprRange.end, varName));

            /* ASSERTIVE */
            var rawExprString = rawProgram.Substring(startPos, endPos - startPos);

            var replaceStart = exprRange.start - startPos;
            var replaceEnd = exprRange.end - startPos;
            var replacedExprString = rawProgram.Substring(startPos, endPos - startPos);
            replacedExprString = replacedExprString.Remove(replaceStart, replaceEnd - replaceStart)
                .Insert(replaceStart, varName);
            var assert = $"\n assert {replacedExprString} == {rawExprString};";

            var divisorIndex = stmtDivisors.FindIndex(divisor => divisor >= exp.tok.pos);
            if (divisorIndex < 1) return;
            var assertPos = stmtDivisors[divisorIndex - 1] + 1;

            AssertSourceEdits.Add(new SourceEdit(assertPos, assert));
        }
    }
}
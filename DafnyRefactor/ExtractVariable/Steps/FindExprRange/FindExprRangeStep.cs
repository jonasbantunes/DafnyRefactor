using System;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.ExtractVariable
{
    /// <summary>
    ///     A <c>RefactorStep</c> that locate the start and end of selected expression, according with AST.
    /// </summary>
    public class FindExprRangeStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        protected Expression endExpr;
        protected FindExprNeighbours endFinder;
        protected TState inState;
        protected Expression startExpr;
        protected FindExprNeighbours startFinder;


        public override void Handle(TState state)
        {
            if (state == null || state.Program == null || state.SourceCode == null || state.EvUserSelection == null ||
                state.EvStmt == null)
                throw new ArgumentNullException();

            inState = state;

            Setup();
            if (state.Errors.Count > 0) return;
            FindStart();
            if (state.Errors.Count > 0) return;
            FindEnd();
            if (state.Errors.Count > 0) return;
            CalcExprRange();

            base.Handle(state);
        }

        protected void Setup()
        {
            startFinder = new FindExprNeighbours(inState.EvStmt, inState.EvUserSelection.start);
            startFinder.Execute();
            endFinder = new FindExprNeighbours(inState.EvStmt, inState.EvUserSelection.end);
            endFinder.Execute();

            if (startFinder.LeftExpr == null && startFinder.RightExpr == null ||
                endFinder.LeftExpr == null && endFinder.RightExpr == null)
            {
                inState.AddError(ExtractVariableErrorMsg.NotAnExpr());
            }
        }

        protected void FindStart()
        {
            if (startFinder.LeftExpr is NegationExpression negationExpr &&
                !IsSubExprChecker.IsSubExpr(endFinder.LeftExpr, negationExpr))
            {
                inState.AddError(ExtractVariableErrorMsg.NegationSlice());
                return;
            }

            if (startFinder.RightExpr is BinaryExpr)
            {
                inState.AddError(ExtractVariableErrorMsg.StartsWithBinExp());
                return;
            }

            if (startFinder.RightExpr is ExprDotName exprDotName)
            {
                FindStartExprDotName(exprDotName);
                return;
            }

            startExpr = startFinder.RightExpr;
        }

        protected void FindEnd()
        {
            if (endFinder.LeftExpr == null || endFinder.LeftExpr is BinaryExpr ||
                endFinder.LeftExpr is NegationExpression)
            {
                FindEndInvalidLeft();
                return;
            }

            if (endFinder.LeftExpr == startFinder.LeftExpr && endFinder.RightExpr == startFinder.RightExpr)
            {
                endExpr = endFinder.RightExpr;
                return;
            }

            endExpr = endFinder.LeftExpr;
        }

        protected void CalcExprRange()
        {
            var selectionStart = inState.EvUserSelection.start;
            var selectionEnd = inState.EvUserSelection.end;
            var exprStart = startExpr.tok.pos;
            var exprEnd = endExpr.tok.pos + endExpr.tok.val.Length;

            var startPos = selectionStart <= exprStart ? selectionStart : exprStart;
            var endPos = selectionEnd >= exprEnd ? selectionEnd : exprEnd;
            inState.EvExprRange = new Range(startPos, endPos);
        }

        protected void FindStartExprDotName(ExprDotName exprDotName)
        {
            var lhs = exprDotName.Lhs;
            while (lhs is ExprDotName subDotName)
            {
                lhs = subDotName.Lhs;
            }

            var startPos = lhs.tok.pos;
            var endPos = lhs.tok.pos + lhs.tok.val.Length;

            if (startPos > inState.EvUserSelection.start || inState.EvUserSelection.start > endPos)
            {
                inState.AddError(ExtractVariableErrorMsg.ObjectSlice());
                return;
            }

            startExpr = lhs;
        }


        protected void FindEndInvalidLeft()
        {
            if (endFinder.RightExpr is ExprDotName)
            {
                FindEndExprDotName();
                return;
            }

            var tokPos = endFinder.RightExpr.tok.pos;
            var endTokPos = endFinder.RightExpr.tok.pos + endFinder.RightExpr.tok.val.Length;
            if (tokPos < inState.EvUserSelection.end && inState.EvUserSelection.end < endTokPos)
            {
                endExpr = endFinder.RightExpr;
            }
            else
            {
                inState.AddError(ExtractVariableErrorMsg.EndsWithBinExp());
            }
        }

        protected void FindEndExprDotName()
        {
            var lhs = endFinder.RightExpr;
            while (lhs is ExprDotName subDotName)
            {
                var lhsPos = subDotName.tok.pos;
                var lhsEndPos = subDotName.tok.pos + subDotName.tok.val.Length;
                if (lhsPos < inState.EvUserSelection.end && inState.EvUserSelection.end < lhsEndPos) break;

                lhs = subDotName.Lhs;
            }

            endExpr = lhs;
        }
    }
}
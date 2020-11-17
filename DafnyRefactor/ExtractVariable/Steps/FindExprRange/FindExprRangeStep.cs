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
        private Expression _endExpr;
        private FindExprNeighbours _endFinder;
        private TState _inState;
        private Expression _startExpr;
        private FindExprNeighbours _startFinder;


        public override void Handle(TState state)
        {
            if (state == null || state.Program == null || state.SourceCode == null || state.EvUserSelection == null ||
                state.EvStmt == null)
                throw new ArgumentNullException();

            _inState = state;

            Setup();
            if (state.Errors.Count > 0) return;
            FindStart();
            if (state.Errors.Count > 0) return;
            FindEnd();
            if (state.Errors.Count > 0) return;
            CalcExprRange();

            base.Handle(state);
        }

        private void Setup()
        {
            _startFinder = new FindExprNeighbours(_inState.EvStmt, _inState.EvUserSelection.start);
            _startFinder.Execute();
            _endFinder = new FindExprNeighbours(_inState.EvStmt, _inState.EvUserSelection.end);
            _endFinder.Execute();

            if (_startFinder.LeftExpr == null && _startFinder.RightExpr == null ||
                _endFinder.LeftExpr == null && _endFinder.RightExpr == null)
            {
                _inState.AddError(ExtractVariableErrorMsg.NotAnExpr());
            }
        }

        private void FindStart()
        {
            if (_startFinder.LeftExpr is NegationExpression negationExpr &&
                !IsSubExprChecker.IsSubExpr(_endFinder.LeftExpr, negationExpr))
            {
                _inState.AddError(ExtractVariableErrorMsg.NegationSlice());
                return;
            }

            if (_startFinder.RightExpr is BinaryExpr)
            {
                _inState.AddError(ExtractVariableErrorMsg.StartsWithBinExp());
                return;
            }

            if (_startFinder.RightExpr is ExprDotName exprDotName)
            {
                FindStartExprDotName(exprDotName);
                return;
            }

            _startExpr = _startFinder.RightExpr;
        }

        private void FindEnd()
        {
            if (_endFinder.LeftExpr == null || _endFinder.LeftExpr is BinaryExpr ||
                _endFinder.LeftExpr is NegationExpression)
            {
                FindEndInvalidLeft();
                return;
            }

            if (_endFinder.LeftExpr == _startFinder.LeftExpr && _endFinder.RightExpr == _startFinder.RightExpr)
            {
                _endExpr = _endFinder.RightExpr;
                return;
            }

            _endExpr = _endFinder.LeftExpr;
        }

        private void CalcExprRange()
        {
            var selectionStart = _inState.EvUserSelection.start;
            var selectionEnd = _inState.EvUserSelection.end;
            var exprStart = _startExpr.tok.pos;
            var exprEnd = _endExpr.tok.pos + _endExpr.tok.val.Length;

            var startPos = selectionStart <= exprStart ? selectionStart : exprStart;
            var endPos = selectionEnd >= exprEnd ? selectionEnd : exprEnd;
            _inState.EvExprRange = new Range(startPos, endPos);
        }

        private void FindStartExprDotName(ExprDotName exprDotName)
        {
            var lhs = exprDotName.Lhs;
            while (lhs is ExprDotName subDotName)
            {
                lhs = subDotName.Lhs;
            }

            var startPos = lhs.tok.pos;
            var endPos = lhs.tok.pos + lhs.tok.val.Length;

            if (startPos > _inState.EvUserSelection.start || _inState.EvUserSelection.start > endPos)
            {
                _inState.AddError(ExtractVariableErrorMsg.ObjectSlice());
                return;
            }

            _startExpr = lhs;
        }


        private void FindEndInvalidLeft()
        {
            if (_endFinder.RightExpr is ExprDotName)
            {
                FindEndExprDotName();
                return;
            }

            var tokPos = _endFinder.RightExpr.tok.pos;
            var endTokPos = _endFinder.RightExpr.tok.pos + _endFinder.RightExpr.tok.val.Length;
            if (tokPos < _inState.EvUserSelection.end && _inState.EvUserSelection.end < endTokPos)
            {
                _endExpr = _endFinder.RightExpr;
            }
            else
            {
                _inState.AddError(ExtractVariableErrorMsg.EndsWithBinExp());
            }
        }

        private void FindEndExprDotName()
        {
            var lhs = _endFinder.RightExpr;
            while (lhs is ExprDotName subDotName)
            {
                var lhsPos = subDotName.tok.pos;
                var lhsEndPos = subDotName.tok.pos + subDotName.tok.val.Length;
                if (lhsPos < _inState.EvUserSelection.end && _inState.EvUserSelection.end < lhsEndPos) break;

                lhs = subDotName.Lhs;
            }

            _endExpr = lhs;
        }
    }
}
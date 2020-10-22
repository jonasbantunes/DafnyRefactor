﻿using System;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public class FindExpressionStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        protected FindExprNeighbours endFinder;
        protected Expression endExpr;
        protected Expression startExpr;
        protected TState inState;
        protected FindExprNeighbours startFinder;


        public override void Handle(TState state)
        {
            if (state == null || state.Program == null || state.RawProgram == null || state.Selection == null ||
                state.ExtractStmt == null)
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
            startFinder = new FindExprNeighbours(inState.ExtractStmt, inState.Selection.start);
            startFinder.Execute();
            endFinder = new FindExprNeighbours(inState.ExtractStmt, inState.Selection.end);
            endFinder.Execute();

            if (startFinder.LeftExpr == null && startFinder.RightExpr == null ||
                endFinder.LeftExpr == null && endFinder.RightExpr == null)
            {
                inState.Errors.Add("Error: selection is not an expression.");
            }
        }

        protected void FindStart()

        {
            if (startFinder.LeftExpr is NegationExpression negationExpr &&
                !IsSubExpr(endFinder.LeftExpr, negationExpr))
            {
                inState.Errors.Add("Error: Selected expression is invalid");
                return;
            }

            if (startFinder.RightExpr is BinaryExpr)
            {
                inState.Errors.Add("Error: selected expression starts with a binary operand.");
                return;
            }

            if (startFinder.RightExpr is ExprDotName exprDotName)
            {
                FindStartExprDotName(exprDotName);
                return;
            }

            startExpr = startFinder.RightExpr;
        }

        protected void FindStartExprDotName(ExprDotName exprDotName)
        {
            var lhs = exprDotName.Lhs;
            while (!(lhs is NameSegment))
            {
                var subDotName = (ExprDotName) lhs;
                lhs = subDotName.Lhs;
            }

            var startPos = lhs.tok.pos;
            var endPos = lhs.tok.pos + lhs.tok.val.Length;

            if (startPos > inState.Selection.start || inState.Selection.start > endPos)
            {
                inState.Errors.Add("Error: Selection should start on beginning of object");
                return;
            }

            startExpr = lhs;
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

        protected void FindEndInvalidLeft()
        {
            if (endFinder.RightExpr is ExprDotName)
            {
                FindEndExprDotName();
                return;
            }

            var tokPos = endFinder.RightExpr.tok.pos;
            var endTokPos = endFinder.RightExpr.tok.pos + endFinder.RightExpr.tok.val.Length;
            if (tokPos < inState.Selection.end && inState.Selection.end < endTokPos)
            {
                endExpr = endFinder.RightExpr;
            }
            else
            {
                inState.Errors.Add("Error: selected expression ends with a operand.");
            }
        }

        protected void FindEndExprDotName()
        {
            var lhs = endFinder.RightExpr;
            while (!(lhs is NameSegment))
            {
                var subDotName = (ExprDotName) lhs;
                var lhsPos = subDotName.tok.pos;
                var lhsEndPos = subDotName.tok.pos + subDotName.tok.val.Length;
                if (lhsPos < inState.Selection.end && inState.Selection.end < lhsEndPos) break;

                lhs = subDotName.Lhs;
            }

            endExpr = lhs;
        }

        protected void CalcExprRange()
        {
            var selectionStart = inState.Selection.start;
            var selectionEnd = inState.Selection.end;
            var exprStart = startExpr.tok.pos;
            var exprEnd = endExpr.tok.pos + endExpr.tok.val.Length;

            var startPos = selectionStart <= exprStart ? selectionStart : exprStart;
            var endPos = selectionEnd >= exprEnd ? selectionEnd : exprEnd;
            inState.ExprRange = new Range(startPos, endPos);
        }

        protected bool IsSubExpr(Expression subExpr, Expression rootExpr)
        {
            var checker = new IsSubExprVisitor(subExpr, rootExpr);
            checker.Execute();
            return checker.IsSubExpr;
        }
    }

    internal class IsSubExprVisitor : DafnyVisitor
    {
        protected readonly Expression rootExpr;
        protected readonly Expression subExpr;

        public IsSubExprVisitor(Expression subExpr, Expression rootExpr)
        {
            if (subExpr == null || rootExpr == null) throw new ArgumentNullException();

            this.subExpr = subExpr;
            this.rootExpr = rootExpr;
        }

        public bool IsSubExpr { get; protected set; }

        public void Execute()
        {
            IsSubExpr = false;
            Visit(rootExpr);
        }

        protected override void Visit(Expression exp)
        {
            if (exp == subExpr)
            {
                IsSubExpr = true;
                return;
            }

            base.Visit(exp);
        }
    }
}
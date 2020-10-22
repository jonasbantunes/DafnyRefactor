using System;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public class FindExpressionStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        protected FindExprVisitor startFinder;
        protected FindExprVisitor endFinder;
        protected Expression exprStart;
        protected Expression exprEnd;
        protected TState inState;


        public override void Handle(TState state)
        {
            if (state == null || state.Program == null || state.RawProgram == null || state.Range == null ||
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
            startFinder = new FindExprVisitor(inState.ExtractStmt, inState.Range.start);
            startFinder.Execute();
            endFinder = new FindExprVisitor(inState.ExtractStmt, inState.Range.end);
            endFinder.Execute();

            if ((startFinder.LeftExpr == null && startFinder.RightExpr == null) ||
                (endFinder.LeftExpr == null && endFinder.RightExpr == null))
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
                var lhs = exprDotName.Lhs;
                while (!(lhs is NameSegment))
                {
                    var subDotName = (ExprDotName) lhs;
                    lhs = subDotName.Lhs;
                }

                var startPos = lhs.tok.pos;
                var endPos = lhs.tok.pos + lhs.tok.val.Length;

                if (startPos > inState.Range.start || inState.Range.start > endPos)
                {
                    inState.Errors.Add("Error: Selection should start on beginning of object");
                    return;
                }

                exprStart = lhs;
                return;
            }

            exprStart = startFinder.RightExpr;
        }

        protected void FindEnd()
        {
            if (endFinder.LeftExpr == null || endFinder.LeftExpr is BinaryExpr ||
                endFinder.LeftExpr is NegationExpression)
            {
                if (endFinder.RightExpr is ExprDotName)
                {
                    var lhs = endFinder.RightExpr;
                    while (!(lhs is NameSegment))
                    {
                        var subDotName = (ExprDotName) lhs;
                        var lhsPos = subDotName.tok.pos;
                        var lhsEndPos = subDotName.tok.pos + subDotName.tok.val.Length;
                        if (lhsPos < inState.Range.end && inState.Range.end < lhsEndPos) break;

                        lhs = subDotName.Lhs;
                    }

                    exprEnd = lhs;
                    return;
                }

                var tokPos = endFinder.RightExpr.tok.pos;
                var endTokPos = endFinder.RightExpr.tok.pos + endFinder.RightExpr.tok.val.Length;
                if (tokPos < inState.Range.end && inState.Range.end < endTokPos)
                {
                    exprEnd = endFinder.RightExpr;
                }
                else
                {
                    inState.Errors.Add("Error: selected expression ends with a operand.");
                }
            }
            else if (endFinder.LeftExpr == startFinder.LeftExpr && endFinder.RightExpr == startFinder.RightExpr)
            {
                exprEnd = endFinder.RightExpr;
            }
            else
            {
                exprEnd = endFinder.LeftExpr;
            }
        }

        protected void CalcExprRange()
        {
            var startPos = inState.Range.start <= exprStart.tok.pos ? inState.Range.start : exprStart.tok.pos;
            var endPos = inState.Range.end >= exprEnd.tok.pos + exprEnd.tok.val.Length
                ? inState.Range.end
                : exprEnd.tok.pos + exprEnd.tok.val.Length;
            inState.ExprRange = new Range(startPos, endPos);
        }

        protected bool IsSubExpr(Expression subExpr, Expression rootExpr)
        {
            var checker = new IsSubExprVisitor(subExpr, rootExpr);
            checker.Execute();
            return checker.IsSubExpr;
        }
    }

    // TODO: Change this to accept rootExpr only.
    public class FindExprVisitor : DafnyVisitor
    {
        protected Statement rootStmt;
        protected Expression rootExpr;
        protected int position;
        public Expression LeftExpr { get; protected set; }
        public Expression RightExpr { get; protected set; }

        public FindExprVisitor(Statement rootStmt, int position)
        {
            this.rootStmt = rootStmt;
            this.position = position;
        }

        public FindExprVisitor(Expression rootExpr, int position)
        {
            this.rootExpr = rootExpr;
            this.position = position;
        }

        public void Execute()
        {
            LeftExpr = null;
            RightExpr = null;

            if (rootStmt != null)
            {
                Visit(rootStmt);
            }
            else
            {
                Visit(rootExpr);
            }
        }

        protected override void Visit(BinaryExpr binaryExpr)
        {
            Visit(binaryExpr.E0);

            if (binaryExpr.tok.pos < position)
            {
                LeftExpr = binaryExpr;
            }
            else if (RightExpr == null)
            {
                RightExpr = binaryExpr;
            }

            Visit(binaryExpr.E1);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            var endTokPos = nameSeg.tok.pos + nameSeg.tok.val.Length - 1;
            if (endTokPos < position)
            {
                LeftExpr = nameSeg;
            }
            else if (RightExpr == null)
            {
                RightExpr = nameSeg;
            }
        }

        protected override void Visit(ExprDotName exprDotName)
        {
            var endTokPos = exprDotName.tok.pos + exprDotName.tok.val.Length - 1;
            if (endTokPos < position)
            {
                LeftExpr = exprDotName;
            }
            else if (RightExpr == null)
            {
                RightExpr = exprDotName;
            }
        }

        protected override void Visit(LiteralExpr literalExpr)
        {
            var endTokPos = literalExpr.tok.pos + literalExpr.tok.val.Length - 1;
            if (endTokPos < position)
            {
                LeftExpr = literalExpr;
            }
            else if (RightExpr == null)
            {
                RightExpr = literalExpr;
            }
        }

        protected override void Visit(NegationExpression negationExpr)
        {
            if (negationExpr.tok.pos < position)
            {
                LeftExpr = negationExpr;
            }
            else if (RightExpr == null)
            {
                RightExpr = negationExpr;
            }

            Visit(negationExpr.E);
        }
    }

    public class IsSubExprVisitor : DafnyVisitor
    {
        protected readonly Expression subExpr;
        protected readonly Expression rootExpr;
        public bool IsSubExpr { get; protected set; }

        public IsSubExprVisitor(Expression subExpr, Expression rootExpr)
        {
            if (subExpr == null || rootExpr == null) throw new ArgumentNullException();

            this.subExpr = subExpr;
            this.rootExpr = rootExpr;
        }

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
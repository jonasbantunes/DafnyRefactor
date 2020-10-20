using System;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public class FindExpressionStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null || state.RawProgram == null || state.Range == null ||
                state.ExtractStmt == null)
                throw new ArgumentNullException();

            var startFinder = new FindExprVisitor(state.ExtractStmt, state.Range.start);
            startFinder.Execute();
            var exprStart = startFinder.RightExpr is BinaryExpr ? startFinder.LeftExpr : startFinder.RightExpr;

            var endFinder = new FindExprVisitor(state.ExtractStmt, state.Range.end);
            endFinder.Execute();
            var exprEnd = endFinder.LeftExpr is BinaryExpr ? endFinder.RightExpr : endFinder.LeftExpr;

            var startPos = state.Range.start <= exprStart.tok.pos ? state.Range.start : exprStart.tok.pos;
            var endPos = state.Range.end >= exprEnd.tok.pos + exprEnd.tok.val.Length
                ? state.Range.end
                : exprEnd.tok.pos + exprEnd.tok.val.Length;
            state.ExprRange = new Range(startPos, endPos);

            var exprString = state.RawProgram.Substring(startPos, endPos - startPos);

            base.Handle(state);
        }
    }

    internal class FindExprVisitor : DafnyVisitor
    {
        protected Statement rootStmt;
        protected int position;
        public Expression LeftExpr { get; protected set; }
        public Expression RightExpr { get; protected set; }

        public FindExprVisitor(Statement rootStmt, int position)
        {
            this.rootStmt = rootStmt;
            this.position = position;
        }

        public void Execute()
        {
            LeftExpr = null;
            RightExpr = null;

            Visit(rootStmt);
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

            base.Visit(nameSeg);
        }

        protected override void Visit(ExprDotName exprDotName)
        {
            var endTokPos = exprDotName.tok.pos + exprDotName.tok.val.Length - 1;
            if (endTokPos < position)
            {
                LeftExpr = exprDotName;
                // base.Visit(exprDotName);
                //Visit(exprDotName.Lhs);

                //var lhs = exprDotName.Lhs;
                //while (lhs != null && !(lhs is NameSegment))
                //{
                //    if (lhs is ExprDotName exprDot)
                //    {
                //        lhs = exprDot.Lhs;
                //    }
                //    else
                //    {
                //        lhs = null;
                //    }
                //}

                //LeftExpr = lhs;
            }
            else if (RightExpr == null)
            {
                //RightExpr = exprDotName;
                //base.Visit(exprDotName);
                //RightExpr = LeftExpr;
                //RightExpr = null;

                var lhs = exprDotName.Lhs;
                while (lhs != null && !(lhs is NameSegment))
                {
                    if (lhs is ExprDotName exprDot)
                    {
                        lhs = exprDot.Lhs;
                    }
                    else
                    {
                        lhs = null;
                    }
                }

                RightExpr = lhs;
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

            base.Visit(literalExpr);
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
            // base.Visit(negationExpr);

            if (RightExpr == negationExpr.E)
            {
                RightExpr = negationExpr;
                LeftExpr = null;
            }
        }
    }
}
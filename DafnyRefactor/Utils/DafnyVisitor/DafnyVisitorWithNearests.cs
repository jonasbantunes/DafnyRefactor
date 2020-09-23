using System;
using Microsoft.Boogie;
using Microsoft.Dafny;

namespace Microsoft.DafnyRefactor.Utils
{
    public class DafnyVisitorWithNearests : DafnyVisitor
    {
        protected Statement nearestStmt;
        protected IToken nearestScopeToken;

        protected override void Visit(Statement stmt)
        {
            var oldNearestStmt = nearestStmt;
            nearestStmt = stmt;
            base.Visit(stmt);
            nearestStmt = oldNearestStmt;
        }

        protected override void Visit(BlockStmt block)
        {
            if (block == null) throw new ArgumentNullException();

            // TODO: Improve stack of variables
            var oldNearest = nearestScopeToken;
            nearestScopeToken = block.Tok;
            Traverse(block.Body);
            nearestScopeToken = oldNearest;
        }
    }
}
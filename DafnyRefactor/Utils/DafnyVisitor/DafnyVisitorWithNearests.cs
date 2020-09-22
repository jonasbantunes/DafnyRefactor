using System;
using Microsoft.Dafny;

namespace Microsoft.DafnyRefactor.Utils
{
    public class DafnyVisitorWithNearests : DafnyVisitor
    {
        protected BlockStmt nearestBlockStmt;
        protected Statement nearestStmt;

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
            var oldNearest = nearestBlockStmt;
            nearestBlockStmt = block;
            Traverse(block.Body);
            nearestBlockStmt = oldNearest;
        }
    }
}
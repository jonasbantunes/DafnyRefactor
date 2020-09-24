using System;
using Microsoft.Boogie;
using Microsoft.Dafny;

namespace Microsoft.DafnyRefactor.Utils
{
    public class DafnyVisitorWithNearests : DafnyVisitor
    {
        protected IToken nearestScopeToken;
        protected Statement nearestStmt;

        protected override void Visit(ClassDecl cd)
        {
            if (cd == null) throw new ArgumentNullException();

            var oldNearest = nearestScopeToken;
            nearestScopeToken = cd.tok;
            base.Visit(cd);
            nearestScopeToken = oldNearest;
        }

        protected override void Visit(Statement stmt)
        {
            if (stmt == null) throw new ArgumentNullException();

            var oldNearestStmt = nearestStmt;
            nearestStmt = stmt;
            base.Visit(stmt);
            nearestStmt = oldNearestStmt;
        }

        protected override void Visit(BlockStmt block)
        {
            if (block == null) throw new ArgumentNullException();

            var oldNearest = nearestScopeToken;
            nearestScopeToken = block.Tok;
            Traverse(block.Body);
            nearestScopeToken = oldNearest;
        }
    }
}
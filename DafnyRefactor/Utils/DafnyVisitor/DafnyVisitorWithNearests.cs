using System;
using Microsoft.Boogie;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils
{
    /// <summary>
    ///     An extension of <c>DafnyVisitor</c> that saves some nearest objects from
    ///     tree, such as the nearest statement or scope token.
    /// </summary>
    public abstract class DafnyVisitorWithNearests : DafnyVisitor
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
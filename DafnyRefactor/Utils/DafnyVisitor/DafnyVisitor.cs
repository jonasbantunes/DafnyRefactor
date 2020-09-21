using System.Collections.Generic;
using System.Linq;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils.DafnyVisitor
{
    public class DafnyVisitor
    {
        protected readonly Program program;
        protected BlockStmt nearestBlockStmt;

        public DafnyVisitor(Program program = null)
        {
            this.program = program;
        }

        public virtual void Execute()
        {
            Visit(program);
        }

        // TODO: Improve naming to differenciate between castings (also Visit), vertical (Visit) and horizontal (Traverse) iteration on tree and 
        protected virtual void Visit(Program prog)
        {
            foreach (var tld in prog.DefaultModuleDef.TopLevelDecls)
            {
                Visit(tld);
            }
        }

        protected virtual void Visit(TopLevelDecl tld)
        {
            if (tld is ClassDecl cd)
            {
                Visit(cd);
            }
        }

        protected virtual void Visit(ClassDecl cd)
        {
            Traverse(cd.Members);
        }

        protected virtual void Visit(MemberDecl md)
        {
            if (md is Method mt)
            {
                Visit(mt);
            }
        }

        protected virtual void Visit(Method mt)
        {
            Visit(mt.Body);
        }

        protected virtual void Visit(Statement stmt)
        {
            if (stmt == null) return;

            switch (stmt)
            {
                case VarDeclStmt vds:
                    Visit(vds);
                    break;
                case UpdateStmt us:
                    Visit(us);
                    break;
                case BlockStmt block:
                    Visit(block);
                    break;
                default:
                    Traverse(stmt.SubExpressions?.ToList());
                    Traverse(stmt.SubStatements?.ToList());
                    break;
            }
        }

        protected virtual void Visit(VarDeclStmt vds)
        {
            Traverse(vds.SubStatements?.ToList());
        }

        protected virtual void Visit(UpdateStmt up)
        {
            Traverse(up.SubExpressions?.ToList());
            Traverse(up.SubStatements?.ToList());
        }

        protected virtual void Visit(BlockStmt block)
        {
            // TODO: Improve stack of variables
            var oldNearest = nearestBlockStmt;
            nearestBlockStmt = block;
            Traverse(block.Body);
            nearestBlockStmt = oldNearest;
        }

        protected virtual void Visit(Expression exp)
        {
            if (exp == null) return;

            switch (exp)
            {
                case NameSegment nameSeg:
                    Visit(nameSeg);
                    break;
                case ExprDotName exprDotName:
                    Visit(exprDotName);
                    break;
                default:
                    Traverse(exp.SubExpressions?.ToList());
                    break;
            }
        }

        protected virtual void Visit(NameSegment nameSeg)
        {
            Traverse(nameSeg.SubExpressions?.ToList());
        }

        protected virtual void Visit(ExprDotName exprDotName)
        {
            Traverse(exprDotName.SubExpressions?.ToList());
        }

        protected virtual void Visit(AssignmentRhs rhs)
        {
            if (rhs == null) return;

            switch (rhs)
            {
                default:
                    Traverse(rhs.SubExpressions?.ToList());
                    Traverse(rhs.SubStatements?.ToList());
                    break;
            }
        }

        protected virtual void Traverse(List<MemberDecl> members)
        {
            foreach (var decl in members)
            {
                Visit(decl);
            }
        }

        protected virtual void Traverse(List<Statement> body)
        {
            foreach (var stmt in body)
            {
                Visit(stmt);
            }
        }

        protected virtual void Traverse(List<Expression> exprs)
        {
            foreach (var expr in exprs)
            {
                Visit(expr);
            }
        }

        protected virtual void Traverse(List<AssignmentRhs> rhss)
        {
            foreach (var rhs in rhss)
            {
                Visit(rhs);
            }
        }
    }
}
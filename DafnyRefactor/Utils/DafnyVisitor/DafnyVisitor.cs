using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dafny;

namespace Microsoft.DafnyRefactor.Utils
{
    public class DafnyVisitor
    {
        protected virtual void Visit(Program prog)
        {
            if (prog == null) throw new ArgumentNullException();

            foreach (var tld in prog.DefaultModuleDef.TopLevelDecls)
            {
                Visit(tld);
            }
        }

        protected virtual void Visit(TopLevelDecl tld)
        {
            if (tld == null) throw new ArgumentNullException();

            if (tld is ClassDecl cd)
            {
                Visit(cd);
            }
        }

        protected virtual void Visit(ClassDecl cd)
        {
            if (cd == null) throw new ArgumentNullException();

            Traverse(cd.Members);
        }

        protected virtual void Visit(MemberDecl md)
        {
            if (md == null) throw new ArgumentNullException();

            if (md is Method mt)
            {
                Visit(mt);
            }
        }

        protected virtual void Visit(Method mt)
        {
            if (mt == null) throw new ArgumentNullException();

            Visit(mt.Body);
        }

        protected virtual void Visit(Statement stmt)
        {
            if (stmt == null) throw new ArgumentNullException();

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
                case AssignStmt assignStmt:
                    Visit(assignStmt);
                    break;
                default:
                    Traverse(stmt.SubExpressions?.ToList());
                    Traverse(stmt.SubStatements?.ToList());
                    break;
            }
        }

        protected virtual void Visit(VarDeclStmt vds)
        {
            if (vds == null) throw new ArgumentNullException();

            Traverse(vds.SubStatements?.ToList());
        }

        protected virtual void Visit(UpdateStmt up)
        {
            if (up == null) throw new ArgumentNullException();

            Traverse(up.SubExpressions?.ToList());
            Traverse(up.SubStatements?.ToList());
        }

        protected virtual void Visit(BlockStmt block)
        {
            if (block == null) throw new ArgumentNullException();

            Traverse(block.Body);
        }

        protected virtual void Visit(AssignStmt assignStmt)
        {
            if (assignStmt == null) throw new ArgumentNullException();

            Traverse(assignStmt.SubExpressions?.ToList());
            Traverse(assignStmt.SubExpressions?.ToList());
        }

        protected virtual void Visit(Expression exp)
        {
            if (exp == null) throw new ArgumentNullException();

            switch (exp)
            {
                case NameSegment nameSeg:
                    Visit(nameSeg);
                    break;
                case ExprDotName exprDotName:
                    Visit(exprDotName);
                    break;
                case ApplySuffix applySuffix:
                    Visit(applySuffix);
                    break;
                case MemberSelectExpr memberSelectExpr:
                    Visit(memberSelectExpr);
                    break;
                default:
                    Traverse(exp.SubExpressions?.ToList());
                    break;
            }
        }

        protected virtual void Visit(NameSegment nameSeg)
        {
            if (nameSeg == null) throw new ArgumentNullException();

            Traverse(nameSeg.SubExpressions?.ToList());
        }

        protected virtual void Visit(ExprDotName exprDotName)
        {
            if (exprDotName == null) throw new ArgumentNullException();

            Traverse(exprDotName.SubExpressions?.ToList());
        }

        protected virtual void Visit(ApplySuffix applySuffix)
        {
            if (applySuffix == null) throw new ArgumentNullException();

            Visit(applySuffix.Lhs);
            Traverse(applySuffix.SubExpressions?.ToList());
        }

        protected virtual void Visit(MemberSelectExpr memberSelectExpr)
        {
            if (memberSelectExpr == null) throw new ArgumentNullException();

            Traverse(memberSelectExpr.SubExpressions?.ToList());
        }

        protected virtual void Visit(AssignmentRhs rhs)
        {
            if (rhs == null) throw new ArgumentNullException();

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
            if (members == null) throw new ArgumentNullException();

            foreach (var decl in members)
            {
                Visit(decl);
            }
        }

        protected virtual void Traverse(List<Statement> body)
        {
            if (body == null) throw new ArgumentNullException();

            foreach (var stmt in body)
            {
                Visit(stmt);
            }
        }

        protected virtual void Traverse(List<Expression> exprs)
        {
            if (exprs == null) throw new ArgumentNullException();

            foreach (var expr in exprs)
            {
                Visit(expr);
            }
        }

        protected virtual void Traverse(List<AssignmentRhs> rhss)
        {
            if (rhss == null) throw new ArgumentNullException();

            foreach (var rhs in rhss)
            {
                Visit(rhs);
            }
        }
    }
}
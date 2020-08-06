using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Dafny
{
    public class DafnyVisitor
    {
        protected readonly Program program;

        public DafnyVisitor(Program program)
        {
            this.program = program;
        }

        public virtual void Execute()
        {
            Visit(program);
        }

        // TODO: Improve naming to differenciate between castings (also Visit), vertical (Visit) and horizontal (Traverse) iteration on tree and 
        protected virtual Program Visit(Program prog)
        {
            foreach (TopLevelDecl tld in prog.DefaultModuleDef.TopLevelDecls)
            {
                Visit(tld);
            }

            return program;
        }

        protected virtual TopLevelDecl Visit(TopLevelDecl tld)
        {
            if (tld is ClassDecl cd)
            {
                Visit(cd);
            }

            return tld;
        }

        protected virtual ClassDecl Visit(ClassDecl cd)
        {
            Traverse(cd.Members);
            return cd;
        }

        protected virtual MemberDecl Visit(MemberDecl md)
        {
            if (md is Method mt)
            {
                return Visit(mt);
            }

            return md;
        }

        protected virtual Method Visit(Method mt)
        {
            Visit(mt.Body);

            return mt;
        }

        protected virtual WhileStmt Visit(WhileStmt while_)
        {
            Visit(while_.Body);

            return while_;
        }

        protected virtual BlockStmt Visit(BlockStmt block)
        {
            Traverse(block.Body);
            return block;
        }

        protected virtual IfStmt Visit(IfStmt ifStmt)
        {
            Visit(ifStmt.Guard);

            return ifStmt;
        }

        protected virtual Expression Visit(Expression exp)
        {
            if (exp is ParensExpression parensExps)
            {
                Visit(parensExps);
            }
            else if (exp is BinaryExpr binExp)
            {
                Visit(binExp);
                Visit(binExp);
            }
            else if (exp is NameSegment nameSeg)
            {
                Visit(nameSeg);
            }

            return exp;
        }

        protected virtual ParensExpression Visit(ParensExpression parensExp)
        {
            Visit(parensExp.E);
            return parensExp;
        }

        protected virtual BinaryExpr Visit(BinaryExpr binExp)
        {
            Visit(binExp.E0);
            Visit(binExp.E1);

            return binExp;
        }

        protected virtual NameSegment Visit(NameSegment nameSeg)
        {
            return nameSeg;
        }

        protected virtual Statement Visit(Statement stmt)
        {
            if (stmt is VarDeclStmt vds)
            {
                return Visit(vds);
            }
            else if (stmt is UpdateStmt us)
            {
                return Visit(us);
            }
            else if (stmt is AssertStmt assert)
            {
                return Visit(assert);
            }
            else if (stmt is WhileStmt while_)
            {
                return Visit(while_);
            }

            else if (stmt is IfStmt ifStmt)
            {
                return Visit(ifStmt);
            }

            return stmt;
        }

        protected virtual VarDeclStmt Visit(VarDeclStmt vds)
        {
            if (vds.Update is UpdateStmt up)
            {
                Visit(up);
            }

            return vds;
        }

        protected virtual UpdateStmt Visit(UpdateStmt up)
        {
            Traverse(up.Rhss);
            return up;
        }

        protected virtual AssertStmt Visit(AssertStmt assert) { return assert; }

        protected virtual AssignmentRhs Visit(AssignmentRhs rhs)
        {
            if (rhs is ExprRhs expRhs)
            {
                return Visit(expRhs);
            }

            return rhs;
        }

        protected virtual ExprRhs Visit(ExprRhs expRhs)
        {
            Visit(expRhs.Expr);
            return expRhs;
        }

        protected virtual List<Statement> Traverse(List<Statement> body)
        {
            for (int i = 0; i < body.Count; i++)
            {
                Statement st = body[i];
                body[i] = Visit(st);
            }

            return body;
        }

        protected virtual List<MemberDecl> Traverse(List<MemberDecl> members)
        {
            for (int i = 0; i < members.Count; i++)
            {
                MemberDecl md = members[i];
                members[i] = Visit(md);
            }

            return members;
        }

        protected virtual List<AssignmentRhs> Traverse(List<AssignmentRhs> rhss)
        {
            for (int i = 0; i < rhss.Count; i++)
            {
                rhss[i] = Visit(rhss[i]);
            }

            return rhss;
        }
    }
}

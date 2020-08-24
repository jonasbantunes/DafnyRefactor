using System.Collections.Generic;

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
        protected virtual void Visit(Program prog)
        {
            foreach (TopLevelDecl tld in prog.DefaultModuleDef.TopLevelDecls)
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

        protected virtual void Visit(WhileStmt while_)
        {
            Visit(while_.Body);
        }

        protected virtual void Visit(BlockStmt block)
        {
            Traverse(block.Body);
        }

        protected virtual void Visit(IfStmt ifStmt)
        {
            Visit(ifStmt.Guard);
            Visit(ifStmt.Thn);
            Visit(ifStmt.Els);
        }

        protected virtual void Visit(Expression exp)
        {
            if (exp is ParensExpression parensExps)
            {
                Visit(parensExps);
            }
            else if (exp is BinaryExpr binExp)
            {
                Visit(binExp);
            }
            else if (exp is NameSegment nameSeg)
            {
                Visit(nameSeg);
            }
        }

        protected virtual void Visit(ParensExpression parensExp)
        {
            Visit(parensExp.E);
        }

        protected virtual void Visit(BinaryExpr binExp)
        {
            Visit(binExp.E0);
            Visit(binExp.E1);
        }

        protected virtual void Visit(NameSegment nameSeg) { }

        protected virtual void Visit(Statement stmt)
        {
            if (stmt is VarDeclStmt vds)
            {
                Visit(vds);
            }
            else if (stmt is UpdateStmt us)
            {
                Visit(us);
            }
            else if (stmt is AssertStmt assert)
            {
                Visit(assert);
            }
            else if (stmt is WhileStmt while_)
            {
                Visit(while_);
            }
            else if (stmt is IfStmt ifStmt)
            {
                Visit(ifStmt);
            }
        }

        protected virtual void Visit(VarDeclStmt vds)
        {
            if (vds.Update is UpdateStmt up)
            {
                Visit(up);
            }
        }

        protected virtual void Visit(UpdateStmt up)
        {
            Traverse(up.Rhss);
        }

        protected virtual void Visit(AssertStmt assert)
        {
            Visit(assert.Expr);
        }

        protected virtual void Visit(AssignmentRhs rhs)
        {
            if (rhs is ExprRhs expRhs)
            {
                Visit(expRhs);
            }
        }

        protected virtual void Visit(ExprRhs expRhs)
        {
            Visit(expRhs.Expr);
        }

        protected virtual void Traverse(List<Statement> body)
        {
            foreach (Statement stmt in body)
            {
                Visit(stmt);
            }
        }

        protected virtual void Traverse(List<MemberDecl> members)
        {
            foreach (MemberDecl decl in members)
            {
                Visit(decl);
            }
        }

        protected virtual void Traverse(List<AssignmentRhs> rhss)
        {
            foreach (AssignmentRhs rhs in rhss)
            {
                Visit(rhs);
            }
        }
    }
}

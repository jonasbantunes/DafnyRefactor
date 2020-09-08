using System.Collections.Generic;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils.DafnyVisitor
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

        protected virtual void Visit(WhileStmt @while)
        {
            Visit(@while.Body);
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
            switch (exp)
            {
                case ParensExpression parensExps:
                    Visit(parensExps);
                    break;
                case BinaryExpr binExp:
                    Visit(binExp);
                    break;
                case NameSegment nameSeg:
                    Visit(nameSeg);
                    break;
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

        protected virtual void Visit(NameSegment nameSeg)
        {
        }

        protected virtual void Visit(Statement stmt)
        {
            switch (stmt)
            {
                case VarDeclStmt vds:
                    Visit(vds);
                    break;
                case UpdateStmt us:
                    Visit(us);
                    break;
                case AssertStmt assert:
                    Visit(assert);
                    break;
                case WhileStmt @while:
                    Visit(@while);
                    break;
                case IfStmt ifStmt:
                    Visit(ifStmt);
                    break;
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
            Traverse(up.Lhss);
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

        protected virtual void Traverse(List<Expression> exprs)
        {
            foreach (var expr in exprs)
            {
                Visit(expr);
            }
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
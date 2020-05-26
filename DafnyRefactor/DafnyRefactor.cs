using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.Dafny.DafnyRefactorDriver;

namespace Microsoft.Dafny
{
    // TODO: Change to iterative approach instead of recursive
    public class DafnyRefactor
    {
        // TODO: Check if this can be a Stack<T>
        private List<object> stack;
        private InlineVar inVar;
        private Program program;

        public DafnyRefactor(Program program, string method, string name)
        {
            this.program = program;
            stack = new List<object>();
            inVar = new InlineVar();
            inVar.method = method;
            inVar.name = name;
        }

        public InlineVar retrieveVar()
        {
            stack.Add(program);
            next();
            return inVar;
        }

        private void next()
        {
            var el = stack.Last();

            if (el is Program prog)
            {
                next(prog);
            }
            else if (el is ClassDecl cd)
            {
                next(cd);
            }
            else if (el is Method mt)
            {
                next(mt);
            }
            else if (el is VarDeclStmt vds)
            {
                next(vds);
            }
            else if (el is UpdateStmt us)
            {
                next(us);
            }

            stack.RemoveAt(stack.Count - 1);
        }

        private void next(Program prog)
        {
            foreach (TopLevelDecl tld in prog.DefaultModuleDef.TopLevelDecls)
            {
                this.stack.Add(tld);
                this.next();
            }
        }

        private void next(ClassDecl cd)
        {
            if (cd.Name == "_default")
            {
                foreach (MemberDecl md in cd.Members)
                {
                    if (md.Name == inVar.method)
                    {
                        stack.Add(md);
                        next();
                    }
                }
            }
        }

        private void next(Method mt)
        {
            foreach (Statement st in mt.Body.Body)
            {
                stack.Add(st);
                next();
            }
        }

        private void next(VarDeclStmt vds)
        {
            if (vds.Update is UpdateStmt up)
            {
                for (int i = 0; i < up.Lhss.Count; i++)
                {
                    if (up.Lhss[i] is AutoGhostIdentifierExpr agie && agie.Name == inVar.name)
                    {
                        ExprRhs erhs = (ExprRhs)up.Rhss[i];
                        inVar.expr = erhs.Expr;
                    }
                }
            }
        }

        private void next(UpdateStmt up)
        {
            for (int i = 0; i < up.Lhss.Count; i++)
            {
                if (up.Lhss[i] is NameSegment nm && nm.Name == inVar.name)
                {
                    if (inVar.expr == null && up.Rhss[i] is ExprRhs erhs)
                    {
                        inVar.expr = erhs.Expr;
                    }
                    else
                    {
                        inVar.isUpdated = true;
                    }
                }
            }
        }
    }
}

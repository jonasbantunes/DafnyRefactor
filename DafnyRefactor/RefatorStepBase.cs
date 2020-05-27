using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dafny
{
    // TODO: Change to iterative approach instead of recursive
    public class RefatorStepBase
    {
        // TODO: Check if this can be a Stack<T>
        protected List<object> stack;
        protected Program program;

        public RefatorStepBase(Program program)
        {
            this.program = program;
            stack = new List<object>();
        }

        public void execute()
        {
            stack.Add(program);
            next();
        }

        protected virtual void next()
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

        protected virtual void next(Program prog)
        {
            foreach (TopLevelDecl tld in prog.DefaultModuleDef.TopLevelDecls)
            {
                stack.Add(tld);
                next();
            }
        }

        protected virtual void next(ClassDecl cd)
        {
            foreach (MemberDecl md in cd.Members)
            {
                stack.Add(md);
                next();
            }
        }

        protected virtual void next(Method mt)
        {
            foreach (Statement st in mt.Body.Body)
            {
                stack.Add(st);
                next();
            }
        }

        protected virtual void next(VarDeclStmt vds)
        {
            if (vds.Update is UpdateStmt up)
            {
                stack.Add(up);
                next();
            }
        }

        protected virtual void next(UpdateStmt up) { }
    }
}

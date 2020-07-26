namespace Microsoft.Dafny
{
    // TODO: Change to iterative approach instead of recursive
    public class DafnyProgramVisitor
    {
        protected Program program;

        public DafnyProgramVisitor(Program program)
        {
            this.program = program;
        }

        public void execute()
        {
            next(program);
        }

        protected virtual Program next(Program prog)
        {
            foreach (TopLevelDecl tld in prog.DefaultModuleDef.TopLevelDecls)
            {
                next(tld);
            }

            return program;
        }

        protected virtual TopLevelDecl next(TopLevelDecl tld)
        {
            if (tld is ClassDecl cd)
            {
                next(cd);
            }

            return tld;
        }

        protected virtual ClassDecl next(ClassDecl cd)
        {
            for (int i = 0; i < cd.Members.Count; i++)
            {
                MemberDecl md = cd.Members[i];
                cd.Members[i] = next(md);
            }

            return cd;
        }

        protected virtual MemberDecl next(MemberDecl md)
        {
            if (md is Method mt)
            {
                return next(mt);
            }

            return md;
        }

        protected virtual Method next(Method mt)
        {
            for (int i = 0; i < mt.Body.Body.Count; i++)
            {
                Statement st = mt.Body.Body[i];
                mt.Body.Body[i] = next(st);
            }

            return mt;
        }

        protected virtual Statement next(Statement stmt)
        {
            if (stmt is VarDeclStmt vds)
            {
                return next(vds);
            }
            else if (stmt is UpdateStmt us)
            {
                return next(us);
            }
            else if (stmt is AssertStmt assert)
            {
                return next(assert);
            }
            else if (stmt is WhileStmt while_)
            {
                return next(while_);
            }

            return stmt;
        }

        protected virtual VarDeclStmt next(VarDeclStmt vds)
        {
            if (vds.Update is UpdateStmt up)
            {
                next(up);
            }

            return vds;
        }

        protected virtual UpdateStmt next(UpdateStmt up) { return up; }

        protected virtual AssertStmt next(AssertStmt assert) { return assert; }

        protected virtual WhileStmt next(WhileStmt while_)
        {
            foreach (Statement stmt in while_.Body.Body)
            {
                next(stmt);
            }

            return while_;
        }
    }
}

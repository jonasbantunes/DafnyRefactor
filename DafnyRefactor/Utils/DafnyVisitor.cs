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
            for (int i = 0; i < cd.Members.Count; i++)
            {
                MemberDecl md = cd.Members[i];
                cd.Members[i] = Visit(md);
            }

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
            for (int i = 0; i < mt.Body.Body.Count; i++)
            {
                Statement st = mt.Body.Body[i];
                mt.Body.Body[i] = Visit(st);
            }

            return mt;
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

        protected virtual UpdateStmt Visit(UpdateStmt up) { return up; }

        protected virtual AssertStmt Visit(AssertStmt assert) { return assert; }

        protected virtual WhileStmt Visit(WhileStmt while_)
        {
            foreach (Statement stmt in while_.Body.Body)
            {
                Visit(stmt);
            }

            return while_;
        }
    }
}

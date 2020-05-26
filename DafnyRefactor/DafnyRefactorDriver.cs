using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Microsoft.Dafny
{
    public class DafnyRefactorDriver : DafnyDriver
    {
        public static new int Main(string[] args)
        {
            int ret = 0;
            var thread = new System.Threading.Thread(
              new System.Threading.ThreadStart(() =>
              { ret = ThreadMain(args); }),
                0x10000000); // 256MB stack size to prevent stack overflow
            thread.Start();
            thread.Join();
            return ret;
        }

        public static new int ThreadMain(string[] args)
        {
            Contract.Requires(cce.NonNullElements(args));

            ErrorReporter reporter = new ConsoleErrorReporter();
            DafnyOptions.Install(new DafnyOptions(reporter));

            var dafnyFiles = new List<DafnyFile>();
            dafnyFiles.Add(new DafnyFile(args[0]));

            Program program;
            string err = Dafny.Main.Parse(dafnyFiles, "the program", reporter, out program);
            if (err != null)
            {
                return (int)ExitValue.DAFNY_ERROR;
            }

            refactorInlineTemp(program, "Main", "x");
            Dafny.Main.MaybePrintProgram(program, "-", false);

            return (int)ExitValue.VERIFIED;
        }

        public class InlineVar
        {
            public string name = null;
            public string method = null;
            public Expression expr = null;
            public bool isUpdated = false;
        }

        public static void refactorInlineTemp(Program program, string method, string name)
        {
            var refactor = new DafnyRefactor(program, method, name);
            var inVar = refactor.retrieveVar();

            if (inVar.isUpdated)
            {
                return;
            }

            ClassDecl cd = null;

            foreach (TopLevelDecl tld in program.DefaultModuleDef.TopLevelDecls)
            {
                if (tld.Name == "_default")
                {
                    cd = (ClassDecl)tld;
                    break;
                }
            }

            Method mt = null;
            foreach (MemberDecl md in cd.Members)
            {
                if (md.Name == method)
                {
                    mt = (Method)md;
                }
            }

            foreach (Statement st in mt.Body.Body)
            {
                if (st is VarDeclStmt vds)
                {
                    if (vds.Update is UpdateStmt up)
                    {
                        for (int i = 0; i < up.Rhss.Count; i++)
                        {
                            if (up.Rhss[i] is ExprRhs erhs && erhs.Expr != null)
                            {
                                up.Rhss[i] = new ExprRhs(applyInlineTemp(erhs.Expr, inVar));
                            }
                        }
                    }
                }
                else if (st is UpdateStmt up)
                {
                    for (int i = 0; i < up.Rhss.Count; i++)
                    {
                        if (up.Rhss[i] is ExprRhs erhs && erhs.Expr != null)
                        {
                            up.Rhss[i] = new ExprRhs(applyInlineTemp(erhs.Expr, inVar));
                        }
                    }
                }
            }
        }

        public static Expression applyInlineTemp(Expression exp, InlineVar inVar)
        {
            var outExp = exp;

            if (outExp is NameSegment nameSeg && nameSeg.Name == inVar.name)
            {
                outExp = inVar.expr;
            }
            else if (outExp is BinaryExpr subExp)
            {
                var e0 = applyInlineTemp(subExp.E0, inVar);
                var e1 = applyInlineTemp(subExp.E1, inVar);
                outExp = new BinaryExpr(subExp.tok, subExp.Op, e0, e1);
            }

            return outExp;
        }
    }
}

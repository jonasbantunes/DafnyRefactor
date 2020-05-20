using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Microsoft.Dafny
{
    class DafnyRefactorDriver : DafnyDriver
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
            public Expression expr = null;
            public bool isUpdated = false;
        }

        public static InlineVar retrieveInlineVar(Program program, string method, string var)
        {
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

            InlineVar inVar = new InlineVar();
            inVar.name = var;

            foreach (Statement st in mt.Body.Body)
            {
                if (st is VarDeclStmt vds)
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
                else if (st is UpdateStmt up)
                {
                    for (int i = 0; i < up.Lhss.Count; i++)
                    {
                        if (up.Lhss[i] is NameSegment nm && nm.Name == inVar.name)
                        {
                            if (inVar.expr == null)
                            {
                                if (up.Rhss[i] is ExprRhs erhs)
                                {
                                    inVar.expr = erhs.Expr;
                                }
                            }
                            else
                            {
                                inVar.isUpdated = true;
                            }
                        }
                    }
                }
            }

            return inVar;
        }

        public static void refactorInlineTemp(Program program, string method, string name)
        {
            InlineVar inVar = retrieveInlineVar(program, method, name);

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
                            if (up.Rhss[i] is ExprRhs erhs)
                            {
                                if (erhs.Expr != null)
                                {
                                    up.Rhss[i] = new ExprRhs(applyInlineTemp(erhs.Expr, inVar));
                                }
                            }
                        }
                    }
                }
                else if (st is UpdateStmt up)
                {
                    for (int i = 0; i < up.Rhss.Count; i++)
                    {
                        if (up.Rhss[i] is ExprRhs erhs)
                        {
                            if (erhs.Expr != null)
                            {
                                up.Rhss[i] = new ExprRhs(applyInlineTemp(erhs.Expr, inVar));
                            }
                        }
                    }
                }
            }
        }

        public static Expression applyInlineTemp(Expression exp, InlineVar inVar)
        {
            if (exp is NameSegment nameSeg)
            {
                if (nameSeg.Name == inVar.name)
                {
                    exp = inVar.expr;
                }
            }

            if (exp is BinaryExpr subExp)
            {
                exp = new BinaryExpr(subExp.tok, subExp.Op, applyInlineTemp(subExp.E0, inVar), applyInlineTemp(subExp.E1, inVar));
            }

            return exp;
        }

        //public static void refactorInlineTemp(Program program, List<LocalVariable> list) {
        //  for (int i1 = 0; i1 < (program?.DefaultModuleDef?.TopLevelDecls.Count ?? 0); i1++) {
        //    if (program.DefaultModuleDef.TopLevelDecls[i1] is ClassDecl) {
        //      ClassDecl cd = (ClassDecl)program.DefaultModuleDef.TopLevelDecls[i1];
        //      for (int i2 = 0; i2 < (cd?.Members.Count ?? 0); i2++) {
        //        if (cd.Members[i2] is Method) {
        //          Method mt = (Method)cd.Members[i2];
        //          for (int i3 = 0; i3 < (mt?.Body?.Body?.Count ?? 0); i3++) {
        //            if (mt.Body.Body[i3] is UpdateStmt) {
        //              UpdateStmt us = (UpdateStmt)mt.Body.Body[i3];
        //              for (int i4 = 0; i4 < (list?.Count ?? 0); i4++) {
        //                for (int i5 = 0; i5 < (us?.Rhss.Count); i5++) {
        //                  if (us.Rhss[i5] is ExprRhs) {
        //                    ExprRhs rhs = (ExprRhs)us.Rhss[i5];
        //                    if (rhs.Expr != null) {
        //                      us.Rhss[i5] = new ExprRhs(applyInlineTemp(rhs.Expr, list[i4]));
        //                    }
        //                  }
        //                }
        //              }
        //            }
        //          }
        //        }
        //      }
        //    }
        //  }
        //}

        //public static Expression applyInlineTemp(Expression exp, LocalVariable var) {
        //  if (exp is NameSegment nameSeg) {
        //    if (nameSeg.Name == "x") {
        //      exp = new LiteralExpr(new Microsoft.Boogie.Token(), 4);
        //    }
        //  }

        //  if (exp is BinaryExpr subExp) {
        //    exp = new BinaryExpr(subExp.tok, subExp.Op, applyInlineTemp(subExp.E0, var), applyInlineTemp(subExp.E1, var));
        //  }

        //  return exp;
        //}

        //public static void injectPrint(Program program) {
        //  Microsoft.Boogie.Token tok = new Microsoft.Boogie.Token();
        //  Microsoft.Boogie.Token tokEnd = new Microsoft.Boogie.Token();
        //  List<Expression> exp = new List<Expression>();
        //  exp.Add(new StringLiteralExpr(new Microsoft.Boogie.Token(), "This is an injected hello\\n", false));
        //  PrintStmt testPrint = new PrintStmt(tok, tokEnd, exp);
        //  ((Method)((DefaultClassDecl)program.DefaultModuleDef.TopLevelDecls[0]).Members[0]).Body.Body.Add(testPrint);
        //}
    }
}

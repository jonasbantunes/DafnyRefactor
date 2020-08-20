using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Dafny
{
    public class RemoveRefactoredDeclarationStep : DafnyWithTableVisitor
    {
        protected InlineVariable inlineVar;
        public List<SourceEdit> Edits { get; protected set; }

        public RemoveRefactoredDeclarationStep(Program program, SymbolTable rootTable, InlineVariable inlineVar) : base(program, rootTable)
        {
            this.inlineVar = inlineVar;
        }

        public override void Execute()
        {
            Edits = new List<SourceEdit>();
            base.Execute();
        }

        protected override VarDeclStmt Visit(VarDeclStmt vds)
        {
            if (vds.Locals.Count == 1 && curTable.LookupDeclaration(vds.Locals[0].Name).GetHashCode() == inlineVar.TableDeclaration.GetHashCode())
            {
                Edits.Add(new SourceEdit(vds.Tok.pos, vds.EndTok.pos + 1, ""));
            }
            else if (vds.Update == null)
            {
                var newVds = new VarDeclStmt(vds.Tok, vds.EndTok, vds.Locals.ToList(), null);

                for (int i = vds.Locals.Count - 1; i >= 0; i--)
                {
                    if (vds.Locals[i].Name == inlineVar.Name && curTable.LookupDeclaration(vds.Locals[i].Name).GetHashCode() == inlineVar.TableDeclaration.GetHashCode())
                    {
                        newVds.Locals.RemoveAt(i);

                        var stringWr = new StringWriter();
                        var printer = new Printer(stringWr);
                        printer.PrintStatement(newVds, 0);
                        Edits.Add(new SourceEdit(vds.Tok.pos, vds.EndTok.pos + 1, stringWr.ToString()));

                        break;
                    }
                }
            }
            else
            {
                if (vds.Update is UpdateStmt up)
                {
                    var newUpdate = new UpdateStmt(up.Tok, up.EndTok, up.Lhss.ToList(), up.Rhss.ToList());
                    var newVds = new VarDeclStmt(vds.Tok, vds.EndTok, vds.Locals.ToList(), newUpdate);

                    for (int i = up.Lhss.Count - 1; i >= 0; i--)
                    {
                        if (up.Lhss[i] is AutoGhostIdentifierExpr agie && agie.Name == inlineVar.Name && curTable.LookupDeclaration(agie.Name).GetHashCode() == inlineVar.TableDeclaration.GetHashCode())
                        {
                            newUpdate.Lhss.RemoveAt(i);
                            newUpdate.Rhss.RemoveAt(i);
                            newVds.Locals.RemoveAt(i);

                            var stringWr = new StringWriter();
                            var printer = new Printer(stringWr);
                            printer.PrintStatement(newVds, 0);
                            Edits.Add(new SourceEdit(vds.Tok.pos, vds.EndTok.pos + 1, stringWr.ToString()));

                            break;
                        }
                    }

                }
            }

            return base.Visit(vds);
        }

        protected override UpdateStmt Visit(UpdateStmt up)
        {
            if (up.Lhss.Count == 1 && up.Lhss[0] is NameSegment upNm && curTable.LookupDeclaration(upNm.Name).GetHashCode() == inlineVar.TableDeclaration.GetHashCode())
            {
                Edits.Add(new SourceEdit(upNm.tok.pos, up.EndTok.pos + 1, ""));
            }
            else
            {
                var newUpdate = new UpdateStmt(up.Tok, up.EndTok, up.Lhss.ToList(), up.Rhss.ToList());

                for (int i = up.Lhss.Count - 1; i >= 0; i--)
                {
                    if (up.Lhss[i] is NameSegment nm && nm.Name == inlineVar.Name && curTable.LookupDeclaration(nm.Name).GetHashCode() == inlineVar.TableDeclaration.GetHashCode())
                    {
                        newUpdate.Lhss.RemoveAt(i);
                        newUpdate.Rhss.RemoveAt(i);

                        var stringWr = new StringWriter();
                        var printer = new Printer(stringWr);
                        printer.PrintStatement(newUpdate, 0);
                        Edits.Add(new SourceEdit(up.Lhss[0].tok.pos, up.EndTok.pos + 1, stringWr.ToString()));

                        break;
                    }
                }
            }

            return base.Visit(up);
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using DafnyRefactor.InlineTemp.InlineTable;
using DafnyRefactor.Utils;
using DafnyRefactor.Utils.DafnyVisitor;
using DafnyRefactor.Utils.SourceEdit;
using DafnyRefactor.Utils.SymbolTable;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp.Steps
{
    public class RemoveDeclarationStep : RefactorStep<InlineState>
    {
        public override void Handle(InlineState state)
        {
            var visitor = new RemoveRefactoredDeclarationVisitor(state.program, state.symbolTable, state.inlineSymbol);
            visitor.Execute();
            state.sourceEdits.AddRange(visitor.Edits);
            base.Handle(state);
        }
    }

    internal class RemoveRefactoredDeclarationVisitor : DafnyVisitor
    {
        protected IInlineSymbol inlineVar;
        protected ISymbolTable rootTable;
        public List<SourceEdit> Edits { get; protected set; }

        public RemoveRefactoredDeclarationVisitor(Program program, ISymbolTable rootTable, IInlineSymbol inlineVar) :
            base(program)
        {
            this.inlineVar = inlineVar;
            this.rootTable = rootTable;
        }

        public override void Execute()
        {
            Edits = new List<SourceEdit>();
            base.Execute();
        }

        protected override void Visit(VarDeclStmt vds)
        {
            // TODO: Avoid this repetition on source code
            var curTable = rootTable.FindTable(nearestBlockStmt.Tok.GetHashCode());
            if (vds.Locals.Count == 1 && curTable.LookupSymbol(vds.Locals[0].Name).GetHashCode() ==
                inlineVar.GetHashCode())
            {
                Edits.Add(new SourceEdit(vds.Tok.pos, vds.EndTok.pos + 1, ""));
            }
            else if (vds.Update == null)
            {
                var newVds = new VarDeclStmt(vds.Tok, vds.EndTok, vds.Locals.ToList(), null);

                for (int i = vds.Locals.Count - 1; i >= 0; i--)
                {
                    if (vds.Locals[i].Name == inlineVar.Name &&
                        curTable.LookupSymbol(vds.Locals[i].Name).GetHashCode() ==
                        inlineVar.GetHashCode())
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
                        if (up.Lhss[i] is AutoGhostIdentifierExpr agie && agie.Name == inlineVar.Name &&
                            curTable.LookupSymbol(agie.Name).GetHashCode() ==
                            inlineVar.GetHashCode())
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

            base.Visit(vds);
        }

        protected override void Visit(UpdateStmt up)
        {
            // TODO: Avoid this repetition on source code
            var curTable = rootTable.FindTable(nearestBlockStmt.Tok.GetHashCode());
            if (up.Lhss.Count == 1 && up.Lhss[0] is NameSegment upNm &&
                curTable.LookupSymbol(upNm.Name).GetHashCode() == inlineVar.GetHashCode())
            {
                Edits.Add(new SourceEdit(upNm.tok.pos, up.EndTok.pos + 1, ""));
            }
            else
            {
                var newUpdate = new UpdateStmt(up.Tok, up.EndTok, up.Lhss.ToList(), up.Rhss.ToList());

                for (int i = up.Lhss.Count - 1; i >= 0; i--)
                {
                    if (up.Lhss[i] is NameSegment nm && nm.Name == inlineVar.Name &&
                        curTable.LookupSymbol(nm.Name).GetHashCode() == inlineVar.GetHashCode())
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

            base.Visit(up);
        }
    }
}
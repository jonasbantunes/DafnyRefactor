using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    public class RemoveDeclarationStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.InlineSymbol == null || state.Program == null || state.SymbolTable == null)
                throw new ArgumentNullException();


            var visitor = new RemoveRefactoredDeclarationVisitor(state.Program, state.SymbolTable, state.InlineSymbol);
            visitor.Execute();
            state.SourceEdits.AddRange(visitor.Edits);
            base.Handle(state);
        }
    }

    internal class RemoveRefactoredDeclarationVisitor : DafnyVisitor
    {
        protected IInlineSymbol inlineVar;
        protected Program program;
        protected ISymbolTable rootTable;

        public RemoveRefactoredDeclarationVisitor(Program program, ISymbolTable rootTable, IInlineSymbol inlineVar)
        {
            if (program == null || rootTable == null || inlineVar == null) throw new ArgumentNullException();

            this.program = program;
            this.inlineVar = inlineVar;
            this.rootTable = rootTable;
        }

        public List<SourceEdit> Edits { get; protected set; }

        public virtual void Execute()
        {
            Edits = new List<SourceEdit>();
            Visit(program);
        }

        protected override void Visit(VarDeclStmt vds)
        {
            if (vds == null) throw new ArgumentNullException();

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

                for (var i = vds.Locals.Count - 1; i >= 0; i--)
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

                    for (var i = up.Lhss.Count - 1; i >= 0; i--)
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
            if (up == null) throw new ArgumentNullException();

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

                for (var i = up.Lhss.Count - 1; i >= 0; i--)
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
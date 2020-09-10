using System.Collections.Generic;
using DafnyRefactor.InlineTemp.InlineTable;
using DafnyRefactor.Utils;
using DafnyRefactor.Utils.DafnyVisitor;
using DafnyRefactor.Utils.SourceEdit;
using DafnyRefactor.Utils.SymbolTable;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp.Steps
{
    public class InlineApplyStep : RefactorStep<InlineState>
    {
        public override void Handle(InlineState state)
        {
            var visitor = new InlineApplyVisitor(state.program, state.symbolTable, state.inlineSymbol);
            visitor.Execute();
            state.replaceSourceEdits = visitor.Edits;
            base.Handle(state);
        }
    }

    internal class InlineApplyVisitor : DafnyWithTableVisitor<InlineSymbol>
    {
        protected InlineSymbol inlineVar;
        public List<SourceEdit> Edits { get; protected set; }

        public InlineApplyVisitor(Program program, SymbolTable<InlineSymbol> rootTable, InlineSymbol inlineVar) :
            base(program,
                rootTable)
        {
            this.inlineVar = inlineVar;
        }

        public override void Execute()
        {
            Edits = new List<SourceEdit>();
            base.Execute();
        }

        protected override void Visit(UpdateStmt up)
        {
            Traverse(up.Rhss);
        }


        protected override void Visit(NameSegment nameSeg)
        {
            if (nameSeg.Name == inlineVar.Name && curTable.LookupSymbol(nameSeg.Name).GetHashCode() ==
                inlineVar.GetHashCode())
            {
                Edits.Add(new SourceEdit(nameSeg.tok.pos, nameSeg.tok.pos + nameSeg.tok.val.Length,
                    $"({Printer.ExprToString(inlineVar.expr)})"));
            }

            base.Visit(nameSeg);
        }
    }
}
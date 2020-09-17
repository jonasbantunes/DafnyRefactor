using System.Collections.Generic;
using DafnyRefactor.InlineTemp.InlineTable;
using DafnyRefactor.Utils;
using DafnyRefactor.Utils.DafnyVisitor;
using DafnyRefactor.Utils.SourceEdit;
using DafnyRefactor.Utils.SymbolTable;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp.Steps
{
    public class ReplaceVariableStep : RefactorStep<InlineState>
    {
        public override void Handle(InlineState state)
        {
            var visitor = new InlineApplyVisitor(state.program, state.symbolTable, state.inlineSymbol);
            visitor.Execute();
            state.sourceEdits.AddRange(visitor.Edits);
            base.Handle(state);
        }
    }

    internal class InlineApplyVisitor : DafnyVisitor
    {
        protected IInlineSymbol inlineVar;
        protected ISymbolTable rootTable;

        public InlineApplyVisitor(Program program, ISymbolTable rootTable, IInlineSymbol inlineVar) :
            base(program)
        {
            this.inlineVar = inlineVar;
            this.rootTable = rootTable;
        }

        public List<SourceEdit> Edits { get; protected set; }

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
            // TODO: Avoid this repetition on source code
            var curTable = rootTable.FindTable(nearestBlockStmt.Tok.GetHashCode());
            if (nameSeg.Name == inlineVar.Name && curTable.LookupSymbol(nameSeg.Name).GetHashCode() ==
                inlineVar.GetHashCode())
            {
                Edits.Add(new SourceEdit(nameSeg.tok.pos, nameSeg.tok.pos + nameSeg.tok.val.Length,
                    $"({Printer.ExprToString(inlineVar.Expr)})"));
            }

            base.Visit(nameSeg);
        }
    }
}
using System;
using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    public class ReplaceVariableStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.InlineSymbol == null || state.Program == null || state.SymbolTable == null)
                throw new ArgumentNullException();

            var visitor = new InlineApplyVisitor(state.Program, state.SymbolTable, state.InlineSymbol);
            visitor.Execute();
            state.SourceEdits.AddRange(visitor.Edits);
            base.Handle(state);
        }
    }

    internal class InlineApplyVisitor : DafnyVisitor
    {
        protected IInlineSymbol inlineVar;
        protected Program program;
        protected ISymbolTable rootTable;

        public InlineApplyVisitor(Program program, ISymbolTable rootTable, IInlineSymbol inlineVar)
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

        protected override void Visit(UpdateStmt up)
        {
            if (up == null) throw new ArgumentNullException();

            Traverse(up.Rhss);
        }


        protected override void Visit(NameSegment nameSeg)
        {
            if (nameSeg == null) throw new ArgumentNullException();

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
using System.Collections.Generic;
using DafnyRefactor.Utils.DafnyVisitor;
using DafnyRefactor.Utils.SourceEdit;
using DafnyRefactor.Utils.SymbolTable;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp.Steps
{
    public class InlineRefactorStep : DafnyWithTableVisitor
    {
        protected InlineVariable inlineVar;
        public List<SourceEdit> Edits { get; protected set; }

        public override void Execute()
        {
            Edits = new List<SourceEdit>();
            base.Execute();
        }

        public InlineRefactorStep(Program program, SymbolTable rootTable, InlineVariable inlineVar) : base(program,
            rootTable)
        {
            this.inlineVar = inlineVar;
        }

        protected override void Visit(NameSegment nameSeg)
        {
            if (nameSeg.Name == inlineVar.Name && curTable.LookupDeclaration(nameSeg.Name).GetHashCode() ==
                inlineVar.TableDeclaration.GetHashCode())
            {
                Edits.Add(new SourceEdit(nameSeg.tok.pos, nameSeg.tok.pos + nameSeg.tok.val.Length,
                    $"({Printer.ExprToString(inlineVar.expr)})"));
            }

            base.Visit(nameSeg);
        }
    }
}
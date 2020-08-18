using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Dafny
{
    public class InlineRefactorStep : DafnyWithTableVisitor
    {
        protected string filePath;
        protected InlineVariable inlineVar;
        public List<SourceEdit> Edits { get; protected set; }

        public override void Execute()
        {
            Edits = new List<SourceEdit>();
            base.Execute();
        }

        public InlineRefactorStep(string filePath, Program program, SymbolTable rootTable, InlineVariable inlineVar) : base(program, rootTable)
        {
            this.filePath = filePath;
            this.inlineVar = inlineVar;
        }

        protected override NameSegment Visit(NameSegment nameSeg)
        {
            if (nameSeg.Name == inlineVar.Name && curTable.Lookup(nameSeg.Name).GetHashCode() == inlineVar.tableDeclaration.GetHashCode())
            {
                Edits.Add(new SourceEdit(nameSeg.tok.pos, nameSeg.tok.pos + nameSeg.tok.val.Length, $"({Printer.ExprToString(inlineVar.expr)})"));
            }
            return base.Visit(nameSeg);
        }
    }
}

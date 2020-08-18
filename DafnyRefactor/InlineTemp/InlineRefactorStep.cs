using System;

namespace Microsoft.Dafny
{
    public class InlineRefactorStep : DafnyWithTableVisitor
    {
        protected InlineVariable inlineVar;

        public InlineRefactorStep(Program program, SymbolTable rootTable, InlineVariable inlineVar) : base(program, rootTable)
        {
            this.inlineVar = inlineVar;
        }

        protected override NameSegment Visit(NameSegment nameSeg)
        {
            if (nameSeg.Name == inlineVar.Name && curTable.Lookup(nameSeg.Name).GetHashCode() == inlineVar.tableDeclaration.GetHashCode())
            {
                Console.WriteLine($"({nameSeg.tok.line}:{nameSeg.tok.col})~({nameSeg.tok.line}:{nameSeg.tok.col + (nameSeg.tok.val.Length)}) = {Printer.ExprToString(inlineVar.expr)}");
                Console.WriteLine($"{nameSeg.tok.pos}~{nameSeg.tok.pos + nameSeg.tok.val.Length} = {Printer.ExprToString(inlineVar.expr)}");
            }
            return base.Visit(nameSeg);
        }
    }
}

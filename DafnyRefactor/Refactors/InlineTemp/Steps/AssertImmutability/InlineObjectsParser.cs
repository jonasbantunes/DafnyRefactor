using System;
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     A utility that retrieves all objects present on <c>InlineVariable.Expr</c>.
    /// </summary>
    public class InlineObjectsParser : DafnyVisitor
    {
        private readonly List<InlineObject> _inlineObjects = new List<InlineObject>();
        private readonly IInlineVariable _inlineVariable;

        private InlineObjectsParser(IInlineVariable inlineVariable)
        {
            _inlineVariable = inlineVariable ?? throw new ArgumentNullException();
        }

        private void Execute()
        {
            Visit(_inlineVariable.Expr);
        }

        protected override void Visit(ExprDotName exprDotName)
        {
            var objPrinted = Printer.ExprToString(exprDotName);
            var lhsPrinted = Printer.ExprToString(exprDotName.Lhs);
            var objType = exprDotName.Lhs.Type;
            var memberType = exprDotName.Type;
            var inlineObject = new InlineObject(objPrinted, lhsPrinted, objType, memberType);

            _inlineObjects.Add(inlineObject);

            base.Visit(exprDotName);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            var printed = Printer.ExprToString(nameSeg);
            var objType = nameSeg.Type;
            var inlineObject = new InlineObject(printed, objType, null);

            _inlineObjects.Add(inlineObject);
            base.Visit(nameSeg);
        }

        public static List<InlineObject> Parse(IInlineVariable inlineVariable)
        {
            var parser = new InlineObjectsParser(inlineVariable);
            parser.Execute();
            return parser._inlineObjects;
        }
    }
}
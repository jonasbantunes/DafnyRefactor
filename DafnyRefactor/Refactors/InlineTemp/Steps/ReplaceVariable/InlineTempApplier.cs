using System;
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp
{
    public class InlineTempApplier : DafnyVisitorWithNearests
    {
        private readonly IInlineVariable _inlineVar;
        private readonly Program _program;
        private readonly IRefactorScope _rootScope;
        private List<SourceEdit> _edits;

        private InlineTempApplier(Program program, IRefactorScope rootScope, IInlineVariable inlineVar)
        {
            if (program == null || rootScope == null || inlineVar == null) throw new ArgumentNullException();

            _program = program;
            _inlineVar = inlineVar;
            _rootScope = rootScope;
        }

        private IRefactorScope CurScope => _rootScope.FindScope(nearestScopeToken.GetHashCode());

        private void Execute()
        {
            _edits = new List<SourceEdit>();
            Visit(_program);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            var variable = CurScope.LookupVariable(nameSeg.Name);
            if (variable == null || !variable.Equals(_inlineVar)) return;

            var startPos = nameSeg.tok.pos;
            var endPos = nameSeg.tok.pos + nameSeg.tok.val.Length;
            var exprPrinted = Printer.ExprToString(_inlineVar.Expr);
            var edit = new SourceEdit(startPos, endPos, $"({exprPrinted})");
            _edits.Add(edit);
        }

        public static List<SourceEdit> Apply(Program program, IRefactorScope rootScope, IInlineVariable inlineVar)
        {
            var applier = new InlineTempApplier(program, rootScope, inlineVar);
            applier.Execute();
            return applier._edits;
        }
    }
}
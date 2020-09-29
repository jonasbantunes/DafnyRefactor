using System;
using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     A <c>RefactorStep</c> that replace each occurrence of the located <c>InlineVariable</c> by its expression.
    /// </summary>
    public class ReplaceVariableStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.InlineVariable == null || state.Program == null || state.RootScope == null)
                throw new ArgumentNullException();

            var applier = new InlineTempApplier(state.Program, state.RootScope, state.InlineVariable);
            applier.Execute();
            state.SourceEdits.AddRange(applier.Edits);

            base.Handle(state);
        }
    }

    internal class InlineTempApplier : DafnyVisitorWithNearests
    {
        protected IInlineVariable inlineVar;
        protected Program program;
        protected IRefactorScope rootScope;

        public InlineTempApplier(Program program, IRefactorScope rootScope, IInlineVariable inlineVar)
        {
            if (program == null || rootScope == null || inlineVar == null) throw new ArgumentNullException();

            this.program = program;
            this.inlineVar = inlineVar;
            this.rootScope = rootScope;
        }

        protected IRefactorScope CurScope => rootScope.FindScope(nearestScopeToken.GetHashCode());
        public List<SourceEdit> Edits { get; protected set; }

        public virtual void Execute()
        {
            Edits = new List<SourceEdit>();
            Visit(program);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            var variable = CurScope.LookupVariable(nameSeg.Name);
            if (variable == null || !variable.Equals(inlineVar)) return;

            var startPos = nameSeg.tok.pos;
            var endPos = nameSeg.tok.pos + nameSeg.tok.val.Length;
            var exprPrinted = Printer.ExprToString(inlineVar.Expr);
            var edit = new SourceEdit(startPos, endPos, $"({exprPrinted})");
            Edits.Add(edit);
        }
    }
}
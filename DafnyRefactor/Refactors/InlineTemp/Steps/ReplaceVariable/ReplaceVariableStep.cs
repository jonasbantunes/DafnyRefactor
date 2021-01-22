using System;
using DafnyRefactor.Utils;

namespace DafnyRefactor.InlineTemp
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

            var edits = InlineTempApplier.Apply(state.Program, state.RootScope, state.InlineVariable);
            state.SourceEdits.AddRange(edits);

            base.Handle(state);
        }
    }
}
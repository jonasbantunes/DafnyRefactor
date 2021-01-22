using System;
using DafnyRefactor.Utils;

namespace DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     A <c>RefactorStep</c> that create a list of <c>SourceEdits</c>removing the declaration
    ///     of the refactored variable.
    /// </summary>
    public class RemoveDeclarationStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.InlineVariable == null || state.Program == null || state.RootScope == null)
                throw new ArgumentNullException();

            var removeEdits = DeclarationRemover.Remove(state.Program, state.RootScope, state.InlineVariable);
            state.SourceEdits.AddRange(removeEdits);

            base.Handle(state);
        }
    }
}
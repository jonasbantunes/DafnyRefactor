using System;

namespace DafnyRefactor.Utils
{
    /// <summary>
    ///     A <c>RefactorStep</c> that saves a list of <c>SourceEdit</c> to a file.
    /// </summary>
    public class SaveChangesStep<TState> : RefactorStep<TState> where TState : IRefactorState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Errors == null || state.FilePath == null || state.Options == null ||
                state.SourceEdits == null) throw new ArgumentNullException();

            var changesInvalidateSource = ChangesSaver.Save(state.FilePath, state.SourceEdits, state.Options);
            if (changesInvalidateSource)
            {
                state.AddError("Error: refactor invalidate source");
                return;
            }

            base.Handle(state);
        }
    }
}
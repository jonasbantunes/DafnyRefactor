using System;
using DafnyRefactor.Utils;

namespace DafnyRefactor.MoveMethod
{
    public class UpdateCallsStep<TState> : RefactorStep<TState> where TState : IMoveMethodState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null || state.MvtParam == null ||
                state.SourceCode == null) throw new ArgumentNullException();

            var updatedEdits = CallsUpdater.Update(state.Program, state.SourceCode, state.MvtParam);
            state.SourceEdits.AddRange(updatedEdits);

            base.Handle(state);
        }
    }
}
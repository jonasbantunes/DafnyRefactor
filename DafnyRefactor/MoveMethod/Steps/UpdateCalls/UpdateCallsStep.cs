using System;
using DafnyRefactor.Utils;

namespace DafnyRefactor.MoveMethod
{
    public class UpdateCallsStep<TState> : RefactorStep<TState> where TState : IMoveMethodState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null || state.MvtParam == null ||
                state.MvtSourceCode == null) throw new ArgumentNullException();

            var updatedEdits = CallsUpdater.Update(state.Program, state.MvtSourceCode, state.MvtParam);
            state.SourceEdits.AddRange(updatedEdits);

            base.Handle(state);
        }
    }
}
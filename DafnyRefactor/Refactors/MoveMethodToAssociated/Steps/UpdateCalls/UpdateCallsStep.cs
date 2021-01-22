using System;
using DafnyRefactor.Utils;

namespace DafnyRefactor.MoveMethodToAssociated
{
    public class UpdateCallsStep<TState> : RefactorStep<TState> where TState : IMoveToAssociatedState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.MtaOriginMethod == null || state.MtaTargetField == null ||
                state.Program == null || state.SourceCode == null) throw new ArgumentNullException();

            var edits = CallsUpdater.Update(state.Program, state.SourceCode, state.MtaOriginMethod,
                state.MtaTargetField);
            state.SourceEdits.AddRange(edits);

            base.Handle(state);
        }
    }
}
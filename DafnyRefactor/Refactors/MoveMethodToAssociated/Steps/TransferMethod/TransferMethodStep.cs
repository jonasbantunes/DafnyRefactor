using System;
using DafnyRefactor.Utils;

namespace DafnyRefactor.MoveMethodToAssociated
{
    public class TransferMethodStep<TState> : RefactorStep<TState> where TState : IMoveToAssociatedState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.MtaOriginMethod == null || state.MtaTargetField == null ||
                state.SourceCode == null)
                throw new ArgumentNullException();

            var replaceEdits = MethodUpdater.Update(state.MtaOriginMethod, state.MtaTargetField, state.SourceCode);
            state.SourceEdits.AddRange(replaceEdits);

            base.Handle(state);
        }
    }
}
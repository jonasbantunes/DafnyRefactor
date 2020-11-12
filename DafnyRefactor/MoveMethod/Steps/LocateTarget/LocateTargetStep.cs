using System;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.MoveMethod
{
    public class LocateTargetStep<TState> : RefactorStep<TState> where TState : IMoveMethodState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null) throw new ArgumentNullException();

            var mvtParam = TargetLocator.Locate(state.Program, state.MvtUserTarget);
            if (mvtParam == null)
            {
                state.AddError("Error: can't locate target's parameter to be moved.");
                return;
            }

            state.MvtParam = mvtParam;

            base.Handle(state);
        }
    }
}
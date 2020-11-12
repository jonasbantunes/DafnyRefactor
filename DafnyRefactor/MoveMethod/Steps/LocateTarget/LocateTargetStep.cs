using System;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.MoveMethod
{
    public class LocateTargetStep<TState> : RefactorStep<TState> where TState : IMoveMethodState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null) throw new ArgumentNullException();

            var (param, method) = TargetLocator.Locate(state.Program, state.MvtUserTarget);
            if (param == null || method == null)
            {
                state.AddError("Error: can't locate target's parameter to be moved.");
                return;
            }

            state.MvtMethod = method;
            state.MvtParam = param;

            base.Handle(state);
        }
    }
}
using System;

namespace Microsoft.DafnyRefactor.Utils
{
    public class RefactorStep<TRefactorState> where TRefactorState : IRefactorState
    {
        public RefactorStep<TRefactorState> next;

        public virtual void Handle(TRefactorState state)
        {
            if (state == null || state.Errors == null) throw new ArgumentException();

            if (state.Errors.Count > 0) return;
            next?.Handle(state);
        }
    }
}
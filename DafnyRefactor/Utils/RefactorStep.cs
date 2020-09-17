namespace DafnyRefactor.Utils
{
    public class RefactorStep<TRefactorState> where TRefactorState : IRefactorState
    {
        public RefactorStep<TRefactorState> next;

        public virtual void Handle(TRefactorState state)
        {
            if (state.Errors.Count > 0) return;
            next?.Handle(state);
        }
    }
}
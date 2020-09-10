namespace DafnyRefactor.Utils
{
    public class RefactorStep<TState> where TState : RefactorState
    {
        public RefactorStep<TState> next;

        public virtual void Handle(TState state)
        {
            if (state.errors.Count > 0) return;
            next?.Handle(state);
        }
    }
}
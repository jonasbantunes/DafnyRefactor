using System.IO;

namespace DafnyRefactor.Utils
{
    public class StdinCleaner<TState> : RefactorStep<TState> where TState : RefactorState
    {
        public override void Handle(TState state)
        {
            File.Delete(state.tempFilePath);
            base.Handle(state);
        }
    }
}
using System.IO;

namespace DafnyRefactor.Utils
{
    public class StdinCleaner<TState> : RefactorStep<TState> where TState : IRefactorState
    {
        public override void Handle(TState state)
        {
            File.Delete(state.TempFilePath);
            base.Handle(state);
        }
    }
}
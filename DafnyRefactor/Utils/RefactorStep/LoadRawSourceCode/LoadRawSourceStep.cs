using System;
using System.IO;

namespace DafnyRefactor.Utils
{
    public class LoadRawSourceStep<TState> : RefactorStep<TState> where TState : IRefactorState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.FilePath == null) throw new ArgumentNullException();

            state.SourceCode = File.ReadAllText(state.FilePath);

            base.Handle(state);
        }
    }
}
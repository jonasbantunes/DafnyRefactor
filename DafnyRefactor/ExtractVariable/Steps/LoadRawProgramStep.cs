using System;
using System.IO;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public class LoadRawProgramStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.FilePath == null) throw new ArgumentNullException();

            state.RawProgram = File.ReadAllText(state.FilePath);

            base.Handle(state);
        }
    }
}
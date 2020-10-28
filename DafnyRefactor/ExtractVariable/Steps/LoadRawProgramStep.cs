using System;
using System.IO;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    /// <summary>
    ///     A <c>RefactorStep</c> that loads a string, non-parsed, version of Program.
    /// </summary>
    public class LoadRawProgramStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.FilePath == null) throw new ArgumentNullException();

            state.EvSourceCode = File.ReadAllText(state.FilePath);

            base.Handle(state);
        }
    }
}
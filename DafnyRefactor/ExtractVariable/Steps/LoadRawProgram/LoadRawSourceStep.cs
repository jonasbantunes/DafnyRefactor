using System;
using System.IO;
using DafnyRefactor.Utils;

namespace DafnyRefactor.ExtractVariable
{
    /// <summary>
    ///     A <c>RefactorStep</c> that loads a string, non-parsed, version of Program.
    /// </summary>
    public class LoadRawSourceStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.FilePath == null) throw new ArgumentNullException();

            state.EvSourceCode = File.ReadAllText(state.FilePath);

            base.Handle(state);
        }
    }
}
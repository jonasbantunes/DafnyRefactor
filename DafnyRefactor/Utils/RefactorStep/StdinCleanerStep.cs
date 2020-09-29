using System;
using System.IO;

namespace Microsoft.DafnyRefactor.Utils
{
    /// <summary>
    ///     Remove temporary file created by <c>StdinLoaderStep</c>
    /// </summary>
    public class StdinCleanerStep<TState> : RefactorStep<TState> where TState : IRefactorState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.TempFilePath == null) throw new ArgumentException();

            File.Delete(state.TempFilePath);

            base.Handle(state);
        }
    }
}
﻿using System;
using System.IO;

namespace Microsoft.DafnyRefactor.Utils
{
    public class StdinCleanerStep<TState> : RefactorStep<TState> where TState : IRefactorState
    {
        public override void Handle(TState state)
        {
            if (state?.TempFilePath == null) throw new ArgumentException();

            File.Delete(state.TempFilePath);
            base.Handle(state);
        }
    }
}
﻿using System;
using System.IO;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.MoveMethod
{
    public class LoadRawSourceStep<TState> : RefactorStep<TState> where TState : IMoveMethodState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.FilePath == null) throw new ArgumentNullException();

            state.MvtSourceCode = File.ReadAllText(state.FilePath);

            base.Handle(state);
        }
    }
}
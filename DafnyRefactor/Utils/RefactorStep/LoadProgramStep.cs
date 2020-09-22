using System;
using System.Collections.Generic;
using Microsoft.Dafny;

namespace Microsoft.DafnyRefactor.Utils
{
    public class LoadProgramStep<TState> : RefactorStep<TState> where TState : IRefactorState
    {
        protected TState internalState;

        public override void Handle(TState state)
        {
            if (state == null || state.Errors == null || state.FilePath == null) throw new ArgumentException();

            internalState = state;
            Load();
            base.Handle(state);
        }

        protected void Load()
        {
            ErrorReporter reporter = new ConsoleErrorReporter();
            DafnyOptions.Install(new DafnyOptions(reporter));

            internalState.Program = null;
            if (!IsFileValid())
            {
                internalState.AddError("Program is invalid");
                return;
            }

            var dafnyFiles = new List<DafnyFile>();
            try
            {
                dafnyFiles.Add(new DafnyFile(internalState.FilePath));
            }
            catch (IllegalDafnyFile)
            {
                internalState.AddError("Program is invalid");
                return;
            }

            var err = Main.ParseCheck(dafnyFiles, "the program", reporter, out var tempProgram);
            if (err == null)
            {
                internalState.Program = tempProgram;
            }
        }

        protected bool IsFileValid()
        {
            var res = DafnyDriver.Main(new[] {internalState.FilePath, "/compile:0"});
            return res == 0;
        }
    }
}
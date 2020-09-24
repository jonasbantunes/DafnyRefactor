using System;
using System.Collections.Generic;
using Microsoft.Dafny;

namespace Microsoft.DafnyRefactor.Utils
{
    public class LoadProgramStep<TState> : RefactorStep<TState> where TState : IRefactorState
    {
        protected TState stateRef;

        public override void Handle(TState state)
        {
            if (state == null || state.Errors == null || state.FilePath == null) throw new ArgumentException();

            stateRef = state;
            Load();
            base.Handle(state);
        }

        protected void Load()
        {
            ErrorReporter reporter = new ConsoleErrorReporter();
            DafnyOptions.Install(new DafnyOptions(reporter));

            stateRef.Program = null;
            if (!IsFileValid())
            {
                stateRef.AddError("Program is invalid");
                return;
            }

            var dafnyFiles = new List<DafnyFile>();
            try
            {
                dafnyFiles.Add(new DafnyFile(stateRef.FilePath));
            }
            catch (IllegalDafnyFile)
            {
                stateRef.AddError("Program is invalid");
                return;
            }

            var err = Main.ParseCheck(dafnyFiles, "the program", reporter, out var tempProgram);
            if (err != null)
            {
                stateRef.AddError($"Can't open program: {err}");
            }

            stateRef.Program = tempProgram;
        }

        protected bool IsFileValid()
        {
            var res = DafnyDriver.Main(new[] {stateRef.FilePath, "/compile:0"});
            return res == 0;
        }
    }
}
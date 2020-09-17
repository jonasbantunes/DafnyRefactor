using System.Collections.Generic;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils
{
    public class LoadProgramStep<TState> : RefactorStep<TState> where TState : IRefactorState
    {
        protected TState state;

        public override void Handle(TState state)
        {
            this.state = state;
            Load();
            base.Handle(state);
        }

        protected void Load()
        {
            ErrorReporter reporter = new ConsoleErrorReporter();
            DafnyOptions.Install(new DafnyOptions(reporter));

            state.Program = null;
            if (!IsFileValid())
            {
                state.Errors.Add("Program is invalid");
                return;
            }

            var dafnyFiles = new List<DafnyFile>();
            try
            {
                dafnyFiles.Add(new DafnyFile(state.FilePath));
            }
            catch (IllegalDafnyFile)
            {
                state.Errors.Add("Program is invalid");
                return;
            }

            var err = Main.ParseCheck(dafnyFiles, "the program", reporter, out var tempProgram);
            if (err == null)
            {
                state.Program = tempProgram;
            }
        }

        protected bool IsFileValid()
        {
            var res = DafnyDriver.Main(new[] {state.FilePath, "/compile:0"});
            return res == 0;
        }
    }
}
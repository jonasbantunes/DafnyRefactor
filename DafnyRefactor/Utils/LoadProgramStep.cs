using System.Collections.Generic;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils
{
    public class LoadProgramStep<TState> : RefactorStep<TState> where TState : RefactorState
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

            state.program = null;
            if (!IsFileValid())
            {
                state.errors.Add("Program is invalid");
                return;
            }

            var dafnyFiles = new List<DafnyFile>();
            try
            {
                dafnyFiles.Add(new DafnyFile(state.FilePath));
            }
            catch (IllegalDafnyFile)
            {
                state.errors.Add("Program is invalid");
                return;
            }

            var err = Main.Parse(dafnyFiles, "the program", reporter, out var tempProgram);
            if (err == null)
            {
                state.program = tempProgram;
            }
        }

        protected bool IsFileValid()
        {
            var res = DafnyDriver.Main(new[] {state.FilePath, "/compile:0"});
            return res == 0;
        }
    }
}
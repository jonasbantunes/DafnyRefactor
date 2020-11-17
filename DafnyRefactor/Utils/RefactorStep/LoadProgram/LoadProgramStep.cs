using System;
using System.Collections.Generic;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils
{
    /// <summary>
    ///     A step that loads and parse a program from a valid file.
    ///     <para>
    ///         Parsing is realized by Dafny compiler.
    ///     </para>
    /// </summary>
    public class LoadProgramStep<TState> : RefactorStep<TState> where TState : IRefactorState
    {
        private TState _stateRef;

        public override void Handle(TState state)
        {
            if (state == null || state.Errors == null || state.FilePath == null) throw new ArgumentException();

            _stateRef = state;
            Load();
            base.Handle(state);
        }

        private void Load()
        {
            ErrorReporter reporter = new ConsoleErrorReporter();
            DafnyOptions.Install(new DafnyOptions(reporter));

            _stateRef.Program = null;
            if (!IsFileValid())
            {
                _stateRef.AddError("Program is invalid");
                return;
            }

            var dafnyFiles = new List<DafnyFile>();
            try
            {
                dafnyFiles.Add(new DafnyFile(_stateRef.FilePath));
            }
            catch (IllegalDafnyFile)
            {
                _stateRef.AddError("Program is invalid");
                return;
            }

            var err = Main.ParseCheck(dafnyFiles, "the program", reporter, out var tempProgram);
            if (err != null)
            {
                _stateRef.AddError($"Can't open program: {err}");
            }

            _stateRef.Program = tempProgram;
        }

        private bool IsFileValid()
        {
            var res = DafnyDriver.Main(new[] {_stateRef.FilePath, "/compile:0"});
            return res == 0;
        }
    }
}
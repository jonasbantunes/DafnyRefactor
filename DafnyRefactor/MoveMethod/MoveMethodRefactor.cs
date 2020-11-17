using System;
using System.Collections.Generic;
using System.Linq;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethod
{
    public class MoveMethodRefactor
    {
        private readonly ApplyMoveMethodOptions _options;
        private MoveMethodState _state;
        private List<RefactorStep<MoveMethodState>> _steps;

        public MoveMethodRefactor(ApplyMoveMethodOptions options)
        {
            _options = options ?? throw new ArgumentNullException();
        }

        public int ExitCode { get; private set; }

        public void Apply()
        {
            Setup();
            Execute();
        }

        private void Setup()
        {
            _state = new MoveMethodState(_options);

            _steps = new List<RefactorStep<MoveMethodState>>();
            if (_options.Stdin)
            {
                _steps.Add(new StdinLoaderStep<MoveMethodState>());
            }

            _steps.Add(new LoadProgramStep<MoveMethodState>());
            _steps.Add(new LoadRawSourceStep<MoveMethodState>());
            _steps.Add(new ParseStmtDivisorsStep<MoveMethodState>());
            _steps.Add(new ParseInstancePositionStep<MoveMethodState>());
            _steps.Add(new LocateTargetStep<MoveMethodState>());
            _steps.Add(new CheckClassSignatureStep<MoveMethodState>());
            _steps.Add(new MoveToTargetStep<MoveMethodState>());
            _steps.Add(new UpdateCallsStep<MoveMethodState>());
            _steps.Add(new SaveChangesStep<MoveMethodState>());
            if (_options.Stdin)
            {
                _steps.Add(new StdinCleanerStep<MoveMethodState>());
            }

            for (var i = 0; i < _steps.Count - 1; i++)
            {
                _steps[i].next = _steps[i + 1];
            }
        }

        private void Execute()
        {
            _steps.First().Handle(_state);

            if (_state.Errors.Count > 0)
            {
                foreach (var error in _state.Errors)
                {
                    DafnyRefactorDriver.consoleError.WriteLine(error);
                }

                ExitCode = (int) DafnyDriver.ExitValue.DAFNY_ERROR;
                return;
            }

            ExitCode = (int) DafnyDriver.ExitValue.VERIFIED;
        }
    }
}
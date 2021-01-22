using System;
using System.Collections.Generic;
using System.Linq;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethodToAssociated
{
    public class MoveToAssociatedRefactor
    {
        private readonly ApplyMoveToAssociatedOptions _options;
        private MoveToAssociatedState _state;
        private List<RefactorStep<MoveToAssociatedState>> _steps;
        private int _exitCode;

        private MoveToAssociatedRefactor(ApplyMoveToAssociatedOptions options)
        {
            _options = options ?? throw new ArgumentNullException();
        }

        private void Apply()
        {
            Setup();
            Execute();
        }

        private void Setup()
        {
            _state = new MoveToAssociatedState(_options);

            _steps = new List<RefactorStep<MoveToAssociatedState>>();
            if (_options.Stdin)
            {
                _steps.Add(new StdinLoaderStep<MoveToAssociatedState>());
            }

            _steps.Add(new LoadProgramStep<MoveToAssociatedState>());
            _steps.Add(new LoadRawSourceStep<MoveToAssociatedState>());
            _steps.Add(new ParseStmtDivisorsStep<MoveToAssociatedState>());
            _steps.Add(new ParsePositionStep<MoveToAssociatedState>());
            _steps.Add(new LocateOriginStep<MoveToAssociatedState>());
            _steps.Add(new LocateTargetStep<MoveToAssociatedState>());
            _steps.Add(new AssertFieldImmutabilityStep<MoveToAssociatedState>());
            _steps.Add(new TransferMethodStep<MoveToAssociatedState>());
            _steps.Add(new UpdateCallsStep<MoveToAssociatedState>());
            _steps.Add(new SaveChangesStep<MoveToAssociatedState>());
            if (_options.Stdin)
            {
                _steps.Add(new StdinCleanerStep<MoveToAssociatedState>());
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

                _exitCode = (int) DafnyDriver.ExitValue.DAFNY_ERROR;
                return;
            }

            _exitCode = (int) DafnyDriver.ExitValue.VERIFIED;
        }

        public static int DoRefactor(ApplyMoveToAssociatedOptions options)
        {
            var refactor = new MoveToAssociatedRefactor(options);
            refactor.Apply();
            return refactor._exitCode;
        }
    }
}
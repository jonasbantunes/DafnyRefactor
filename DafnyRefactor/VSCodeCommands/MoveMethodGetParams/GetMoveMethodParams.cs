using System;
using System.Collections.Generic;
using System.Linq;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethod
{
    public class GetMoveMethodParams
    {
        private readonly GetMoveMethodParamsOptions _options;
        private GetMoveMethodState _state;
        private List<RefactorStep<GetMoveMethodState>> _steps;

        public GetMoveMethodParams(GetMoveMethodParamsOptions options)
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
            _state = new GetMoveMethodState(_options);

            _steps = new List<RefactorStep<GetMoveMethodState>>();
            if (_options.Stdin)
            {
                _steps.Add(new StdinLoaderStep<GetMoveMethodState>());
            }

            _steps.Add(new LoadProgramStep<GetMoveMethodState>());
            _steps.Add(new LoadRawSourceStep<GetMoveMethodState>());
            _steps.Add(new ParseMethodPositionStep<GetMoveMethodState>());
            _steps.Add(new FindMethodStep<GetMoveMethodState>());
            if (_options.Stdin)
            {
                _steps.Add(new StdinCleanerStep<GetMoveMethodState>());
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
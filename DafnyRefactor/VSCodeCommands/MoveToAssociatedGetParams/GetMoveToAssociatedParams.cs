using System.Collections.Generic;
using System.Linq;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethodToAssociated
{
    public class GetMoveToAssociatedParams
    {
        private readonly GetMoveToAssociatedParamsOptions _options;
        private IGetMoveToAssociatedState _state;
        private List<RefactorStep<IGetMoveToAssociatedState>> _steps;
        private int _exitCode;

        private GetMoveToAssociatedParams(GetMoveToAssociatedParamsOptions options)
        {
            _options = options;
        }

        private void Apply()
        {
            Setup();
            Execute();
        }

        private void Setup()
        {
            _state = new GetMoveToAssociatedState(_options);

            _steps = new List<RefactorStep<IGetMoveToAssociatedState>>();
            if (_options.Stdin)
            {
                _steps.Add((new StdinLoaderStep<IGetMoveToAssociatedState>()));
            }

            _steps.Add(new LoadProgramStep<IGetMoveToAssociatedState>());
            _steps.Add(new LoadRawSourceStep<IGetMoveToAssociatedState>());
            _steps.Add(new ParseMethodPositionStep<IGetMoveToAssociatedState>());
            _steps.Add(new FindMethodStep<IGetMoveToAssociatedState>());
            if (_options.Stdin)
            {
                _steps.Add(new StdinCleanerStep<IGetMoveToAssociatedState>());
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

        public static int DoRefactor(GetMoveToAssociatedParamsOptions options)
        {
            var refactor = new GetMoveToAssociatedParams(options);
            refactor.Apply();
            return refactor._exitCode;
        }
    }
}
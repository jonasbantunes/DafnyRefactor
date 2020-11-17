using System;
using System.Collections.Generic;
using System.Linq;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.ExtractVariable
{
    public class ExtractVariableRefactor
    {
        private readonly ApplyExtractVariableOptions _options;
        private ExtractVariableState _state;
        private List<RefactorStep<ExtractVariableState>> _steps;

        public ExtractVariableRefactor(ApplyExtractVariableOptions options)
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
            _state = new ExtractVariableState(_options);

            _steps = new List<RefactorStep<ExtractVariableState>>();
            if (_options.Stdin)
            {
                _steps.Add(new StdinLoaderStep<ExtractVariableState>());
            }

            _steps.Add(new LoadProgramStep<ExtractVariableState>());
            _steps.Add(new EvGenerateScopeStep<ExtractVariableState>());
            _steps.Add(new LoadRawSourceStep<ExtractVariableState>());
            _steps.Add(new ParseStmtDivisorsStep<ExtractVariableState>());
            _steps.Add(new ParseSelectionStep<ExtractVariableState>());
            _steps.Add(new FindStatementStep<ExtractVariableState>());
            _steps.Add(new FindExprRangeStep<ExtractVariableState>());
            _steps.Add(new ExtractExprStep<ExtractVariableState>());
            _steps.Add(new ReplaceOccurrencesStep<ExtractVariableState>());
            _steps.Add(new SaveChangesStep<ExtractVariableState>());
            if (_options.Stdin)
            {
                _steps.Add(new StdinCleanerStep<ExtractVariableState>());
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
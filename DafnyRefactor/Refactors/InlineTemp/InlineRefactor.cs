using System;
using System.Collections.Generic;
using System.Linq;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     Apply "Inline Temp" refactor on a <c>Dafny.Program</c> file.
    /// </summary>
    public class InlineRefactor
    {
        private readonly ApplyInlineTempOptions _options;
        private InlineState _state;
        private List<RefactorStep<InlineState>> _steps;

        public InlineRefactor(ApplyInlineTempOptions options)
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
            _state = new InlineState(_options);

            _steps = new List<RefactorStep<InlineState>>();
            if (_options.Stdin)
            {
                _steps.Add(new StdinLoaderStep<InlineState>());
            }

            _steps.Add(new LoadProgramStep<InlineState>());
            _steps.Add(new LoadRawSourceStep<InlineState>());
            _steps.Add(new ParsePositionStep<InlineState>());
            _steps.Add(new ParseStmtDivisorsStep<InlineState>());
            _steps.Add(new GenerateScopeStep<InlineState>());
            _steps.Add(new ParseMethodsStep<InlineState>());
            _steps.Add(new ParseVariablesStep<InlineState>());
            _steps.Add(new LocateVariableStep<InlineState>());
            _steps.Add(new AssertImmutabilitySimplifiedStep<InlineState>());
            _steps.Add(new AssertImmutabilityStep<InlineState>());
            _steps.Add(new ReplaceVariableStep<InlineState>());
            _steps.Add(new RemoveDeclarationStep<InlineState>());
            _steps.Add(new SaveChangesStep<InlineState>());
            if (_options.Stdin)
            {
                _steps.Add(new StdinCleanerStep<InlineState>());
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
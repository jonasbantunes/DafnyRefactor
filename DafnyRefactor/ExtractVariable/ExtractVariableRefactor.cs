using System;
using System.Collections.Generic;
using System.Linq;
using DafnyRefactor;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public class ExtractVariableRefactor
    {
        protected readonly ApplyExtractVariableOptions options;
        protected ExtractVariableState state;
        protected List<RefactorStep<ExtractVariableState>> steps;

        public ExtractVariableRefactor(ApplyExtractVariableOptions options)
        {
            this.options = options ?? throw new ArgumentNullException();
        }

        public int ExitCode { get; protected set; }

        public void Apply()
        {
            Setup();
            Execute();
        }

        protected void Setup()
        {
            state = new ExtractVariableState(options);

            steps = new List<RefactorStep<ExtractVariableState>>();
            if (options.Stdin)
            {
                steps.Add(new StdinLoaderStep<ExtractVariableState>());
            }

            steps.Add(new LoadProgramStep<ExtractVariableState>());
            steps.Add(new EvGenerateScopeStep<ExtractVariableState>());
            steps.Add(new LoadRawProgramStep<ExtractVariableState>());
            steps.Add(new ParseStmtDivisorsStep<ExtractVariableState>());
            steps.Add(new ParseSelectionStep<ExtractVariableState>());
            steps.Add(new FindStatementStep<ExtractVariableState>());
            steps.Add(new FindExprRangeStep<ExtractVariableState>());
            steps.Add(new ExtractVariableStep<ExtractVariableState>());
            steps.Add(new ReplaceOccurrencesStep<ExtractVariableState>());
            steps.Add(new SaveChangesStep<ExtractVariableState>());
            if (options.Stdin)
            {
                steps.Add(new StdinCleanerStep<ExtractVariableState>());
            }

            for (var i = 0; i < steps.Count - 1; i++)
            {
                steps[i].next = steps[i + 1];
            }
        }

        protected void Execute()
        {
            steps.First().Handle(state);

            if (state.Errors.Count > 0)
            {
                foreach (var error in state.Errors)
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
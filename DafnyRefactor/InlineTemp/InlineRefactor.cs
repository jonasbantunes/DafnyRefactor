using System.Collections.Generic;
using System.Linq;
using DafnyRefactor.InlineTemp.Steps;
using DafnyRefactor.Utils;
using DafnyRefactor.Utils.CommandLineOptions;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp
{
    public class InlineRefactor
    {
        protected readonly ApplyInlineTempOptions options;

        public InlineRefactor(ApplyInlineTempOptions options)
        {
            this.options = options;
        }

        public int ExitCode { get; protected set; }

        public void Apply()
        {
            var state = new InlineState(options);

            var steps = new List<RefactorStep<InlineState>>();
            if (options.Stdin)
            {
                steps.Add(new StdinLoader<InlineState>());
            }

            steps.Add(new LoadProgramStep<InlineState>());
            steps.Add(new ParseStmtDivisorsStep<InlineState>());
            steps.Add(new GenerateTableStep<InlineState>());
            steps.Add(new LocateVariableStep<InlineState>());
            steps.Add(new CheckImmutabilityStep<InlineState>());
            steps.Add(new ProveImmutabilityStep<InlineState>());
            steps.Add(new ProveImmutabilityClassicStep<InlineState>());
            steps.Add(new ReplaceVariableStep<InlineState>());
            steps.Add(new RemoveDeclarationStep<InlineState>());
            steps.Add(new SaveChangesStep<InlineState>());
            if (options.Stdin)
            {
                steps.Add(new StdinCleaner<InlineState>());
            }

            for (var i = 0; i < steps.Count - 1; i++)
            {
                steps[i].next = steps[i + 1];
            }

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
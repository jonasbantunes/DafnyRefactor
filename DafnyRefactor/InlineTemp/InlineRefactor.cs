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
        public int ExitCode { get; protected set; }

        public InlineRefactor(ApplyInlineTempOptions options)
        {
            this.options = options;
        }

        public void Apply()
        {
            var state = new InlineState(options);

            var steps = new List<RefactorStep<InlineState>>();
            if (options.Stdin)
            {
                steps.Add(new StdinLoader<InlineState>());
            }

            steps.Add(new LoadProgramStep<InlineState>());
            steps.Add(new GenerateTableStep());
            steps.Add(new LocateVariableStep());
            steps.Add(new CheckImmutabilityStep());
            //steps.Add(new ProveImmutabilityStep());
            steps.Add(new ReplaceVariableStep());
            steps.Add(new RemoveDeclarationStep());
            steps.Add(new SaveChangesStep());
            if (options.Stdin)
            {
                steps.Add(new StdinCleaner<InlineState>());
            }

            for (var i = 0; i < steps.Count - 1; i++)
            {
                steps[i].next = steps[i + 1];
            }

            steps.First().Handle(state);
            if (state.errors.Count > 0)
            {
                foreach (var error in state.errors)
                {
                    DafnyRefactorDriver.consoleError.WriteLine(error);
                }

                ExitCode = (int) DafnyDriver.ExitValue.DAFNY_ERROR;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            if (options.Stdin)
            {
                RefactorFromStdin();
            }
            else
            {
                Refactor();
            }
        }

        // TODO: transform this in a optional step
        protected void RefactorFromStdin()
        {
            var stdinBuilder = new StringBuilder();
            string s;
            while ((s = Console.ReadLine()) != null)
            {
                stdinBuilder.Append((s));
                stdinBuilder.Append(Environment.NewLine);
            }

            var tempPath = Path.GetTempPath() + Guid.NewGuid() + ".dfy";
            File.WriteAllText(tempPath, stdinBuilder.ToString());

            options.FilePath = tempPath;
            Refactor();

            File.Delete(tempPath);
        }

        protected void Refactor()
        {
            var state = new InlineState(options);
            var steps = new List<RefactorStep<InlineState>>
            {
                new DafnyProgramLoader<InlineState>(),
                new GenerateTableStep(),
                new LocateVariableStep(),
                new InlineRetrieveStep(),
                new InlineImmutabilityCheckStep(),
                new InlineApplyStep(),
                new RemoveRefactoredDeclarationStep(),
                new SaveChangesStep()
            };

            for (var i = 0; i < steps.Count - 1; i++)
            {
                steps[i].next = steps[i + 1];
            }

            steps[0].Handle(state);
            if (state.errors.Count > 0)
            {
                foreach (var error in state.errors)
                {
                    DafnyRefactorDriver.consoleOutput.WriteLine(error);
                }

                ExitCode = (int) DafnyDriver.ExitValue.DAFNY_ERROR;
            }
        }
    }
}
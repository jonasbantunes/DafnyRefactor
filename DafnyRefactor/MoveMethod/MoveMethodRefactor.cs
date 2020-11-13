using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.MoveMethod
{
    public class MoveMethodRefactor
    {
        protected readonly ApplyMoveMethodOptions options;
        protected MoveMethodState state;
        protected List<RefactorStep<MoveMethodState>> steps;

        public MoveMethodRefactor(ApplyMoveMethodOptions options)
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
            state = new MoveMethodState(options);

            steps = new List<RefactorStep<MoveMethodState>>();
            if (options.Stdin)
            {
                steps.Add(new StdinLoaderStep<MoveMethodState>());
            }

            steps.Add(new LoadProgramStep<MoveMethodState>());
            steps.Add(new LoadRawSourceStep<MoveMethodState>());
            steps.Add(new ParseStmtDivisorsStep<MoveMethodState>());
            steps.Add(new ParseInstancePositionStep<MoveMethodState>());
            steps.Add(new LocateTargetStep<MoveMethodState>());
            steps.Add(new CheckClassSignatureStep<MoveMethodState>());
            steps.Add(new MoveToTargetStep<MoveMethodState>());
            steps.Add(new UpdateCallsStep<MoveMethodState>());
            steps.Add(new SaveChangesStep<MoveMethodState>());
            if (options.Stdin)
            {
                steps.Add(new StdinCleanerStep<MoveMethodState>());
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
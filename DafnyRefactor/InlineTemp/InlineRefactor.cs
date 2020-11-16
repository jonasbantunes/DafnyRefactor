﻿using System;
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
        protected readonly ApplyInlineTempOptions options;
        protected InlineState state;
        protected List<RefactorStep<InlineState>> steps;

        public InlineRefactor(ApplyInlineTempOptions options)
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
            state = new InlineState(options);

            steps = new List<RefactorStep<InlineState>>();
            if (options.Stdin)
            {
                steps.Add(new StdinLoaderStep<InlineState>());
            }

            steps.Add(new LoadProgramStep<InlineState>());
            steps.Add(new ParseStmtDivisorsStep<InlineState>());
            steps.Add(new GenerateScopeStep<InlineState>());
            steps.Add(new ParseMethodsStep<InlineState>());
            steps.Add(new ParseVariablesStep<InlineState>());
            steps.Add(new LocateVariableStep<InlineState>());
            steps.Add(new AssertImmutabilitySimplifiedStep<InlineState>());
            steps.Add(new AssertImmutabilityStep<InlineState>());
            steps.Add(new ReplaceVariableStep<InlineState>());
            steps.Add(new RemoveDeclarationStep<InlineState>());
            steps.Add(new SaveChangesStep<InlineState>());
            if (options.Stdin)
            {
                steps.Add(new StdinCleanerStep<InlineState>());
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
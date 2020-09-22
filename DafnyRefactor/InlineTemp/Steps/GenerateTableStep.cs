using System;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    public class GenerateTableStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state?.Program == null) throw new ArgumentException();

            // TODO: improve code
            var tableGenerator =
                new ScopeGenerator<InlineScope>(state.Program);
            tableGenerator.Execute();
            state.RootScope = tableGenerator.GeneratedTable;
            base.Handle(state);
        }
    }
}
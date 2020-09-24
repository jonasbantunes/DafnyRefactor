using System;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    public class GenerateScopeStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null) throw new ArgumentException();

            var scopeGenerator = new ScopeGenerator<InlineScope>(state.Program);
            scopeGenerator.Execute();
            state.RootScope = scopeGenerator.GeneratedScope;

            base.Handle(state);
        }
    }
}
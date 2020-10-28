using System;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public class EvGenerateScopeStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null) throw new ArgumentException();

            var scopeGenerator = new ScopeGenerator<RefactorScope>(state.Program);
            scopeGenerator.Execute();
            state.EvRootScope = scopeGenerator.GeneratedScope;

            base.Handle(state);
        }
    }
}
using System;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public class EvGenerateScopeStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null) throw new ArgumentException();

            state.EvRootScope = ScopeGenerator<ExtractVariableScope>.Generate(state.Program);

            base.Handle(state);
        }
    }
}
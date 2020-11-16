using System;
using DafnyRefactor.Utils;

namespace DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     A <c>RefactorStep</c> that generate all <c>RefactorScope</c> from a <c>Dafny.Program</c>.
    /// </summary>
    public class GenerateScopeStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null) throw new ArgumentException();

            state.RootScope = ScopeGenerator<InlineScope>.Generate(state.Program);

            base.Handle(state);
        }
    }
}
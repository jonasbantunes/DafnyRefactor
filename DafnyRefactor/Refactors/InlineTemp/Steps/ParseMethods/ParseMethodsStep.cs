using System;
using DafnyRefactor.Utils;

namespace DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     A <c>RefactorStep</c> that parse all method's info into <c>RefactorMethod</c> objects
    ///     and save each method on it's respective <c>RefactorScope</c>.
    /// </summary>
    public class ParseMethodsStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null || state.RootScope == null) throw new ArgumentNullException();

            MethodParser.Parse(state.Program, state.RootScope);

            base.Handle(state);
        }
    }
}
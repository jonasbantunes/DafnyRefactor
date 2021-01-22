using System;
using DafnyRefactor.Utils;

namespace DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     A <c>RefactorStep</c> that parses all variables of a <c>Dafny.Program</c> into <c>RefactorVariable</c>
    ///     and save each variable into it's respective <c>RefactorState</c>.
    /// </summary>
    public class ParseVariablesStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null || state.RootScope == null)
                throw new ArgumentNullException();

            VariableParser.Parse(state.Program, state.RootScope);

            base.Handle(state);
        }
    }
}
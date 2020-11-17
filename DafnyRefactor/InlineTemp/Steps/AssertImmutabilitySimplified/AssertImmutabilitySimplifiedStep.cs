using System;
using DafnyRefactor.Utils;

namespace DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     A <c>RefactorStep</c> that verifies if all usages of a <c>InlineVariable</c> are constant.
    ///     ///
    ///     <para>
    ///         This is a simplified approach of <c>AssertImmutabilityStep</c>.
    ///     </para>
    /// </summary>
    public class AssertImmutabilitySimplifiedStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Errors == null || state.FilePath == null || state.InlineOptions == null ||
                state.Program == null || state.StmtDivisors == null ||
                state.RootScope == null) throw new ArgumentNullException();

            var assertiveEdits = AssertivesSimplifiedAdder.GetAssertives(state.Program, state.RootScope,
                state.InlineVariable, state.StmtDivisors);

            var isValid = EditsValidator.IsValid(assertiveEdits, state.FilePath);
            if (!isValid)
            {
                state.AddError(
                    $"Error: variable {state.InlineVariable.Name} located on {state.InlineOptions.Position} is not constant according with theorem prover.");
                return;
            }

            base.Handle(state);
        }
    }
}
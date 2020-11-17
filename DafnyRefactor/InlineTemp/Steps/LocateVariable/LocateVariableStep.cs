using System;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     A <c>RefactorStep</c> that locate the variable to be refactored.
    /// </summary>
    public class LocateVariableStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Errors == null || state.InlineOptions == null || state.Program == null ||
                state.RootScope == null) throw new ArgumentNullException();

            var foundVariable = VariableLocator.Locate(state.Program, state.RootScope, state.IvrVariablePos);
            if (foundVariable == null)
            {
                state.AddError(
                    $"Error: can't locate variable on line {state.InlineOptions.Position}.");
                return;
            }

            state.InlineVariable = foundVariable;
            if (state.InlineVariable.IsUpdated)
            {
                state.AddError(
                    $"Error: variable {state.InlineVariable.Name} located on {state.InlineOptions.Position} is not constant.");
                return;
            }

            if (state.InlineVariable.NotAnExpr)
            {
                state.AddError(
                    $"Error: variable {state.InlineVariable.Name} located on {state.InlineOptions.Position} is initialized with an object constructor.");
                return;
            }

            if (state.InlineVariable.Expr == null)
            {
                state.AddError(
                    $"Error: variable {state.InlineVariable.Name} located on {state.InlineOptions.Position} is never initialized.");
                return;
            }

            if (state.InlineVariable.Expr is ApplySuffix)
            {
                state.AddError(
                    $"Error: variable {state.InlineVariable.Name} located on {state.InlineOptions.Position} contains a method call.");
                return;
            }

            base.Handle(state);
        }
    }
}
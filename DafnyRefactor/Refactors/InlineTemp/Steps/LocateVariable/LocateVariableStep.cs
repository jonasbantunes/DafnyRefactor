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
                state.AddError(InlineTempErrorMsg.NotFoundVariable(state.InlineOptions.Position));
                return;
            }

            state.InlineVariable = foundVariable;
            if (state.InlineVariable.IsUpdated)
            {
                state.AddError(InlineTempErrorMsg.NotConstant(state.InlineVariable.Name, state.InlineOptions.Position));
                return;
            }

            if (state.InlineVariable.NotAnExpr)
            {
                state.AddError(InlineTempErrorMsg.InitWithConstructor(state.InlineVariable.Name,
                    state.InlineOptions.Position));
                return;
            }

            if (state.InlineVariable.Expr == null)
            {
                state.AddError(InlineTempErrorMsg.NeverInitialized(state.InlineVariable.Name,
                    state.InlineOptions.Position));
                return;
            }

            if (state.InlineVariable.Expr is ApplySuffix)
            {
                state.AddError(InlineTempErrorMsg.ContainsMethodCall(state.InlineVariable.Name,
                    state.InlineOptions.Position));
                return;
            }

            base.Handle(state);
        }
    }
}
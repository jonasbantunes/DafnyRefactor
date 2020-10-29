using System;
using System.Collections.Generic;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    /// <summary>
    ///     A <c>RefactorStep</c> that replaces all occurences of extracted expression.
    ///     <para>Currently is replacing only the original selection.</para>
    /// </summary>
    public class ReplaceOccurrencesStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        protected List<SourceEdit> assertEdits;
        protected TState inState;
        protected List<SourceEdit> sourceEdits;
        protected EditsValidator validator;

        public override void Handle(TState state)
        {
            if (state == null || state.EvExprRange == null || state.EvStmt == null || state.Program == null ||
                state.EvSourceCode == null || state.EvOptions == null || state.StmtDivisors == null ||
                state.SourceEdits == null || state.EvRootScope == null)
                throw new ArgumentNullException();

            inState = state;

            Replace();
            Validate();
            if (state.Errors.Count > 0) return;

            state.SourceEdits.AddRange(sourceEdits);
            base.Handle(state);
        }

        protected void Replace()
        {
            (sourceEdits, assertEdits) = ExprOcurrencesReplacer.Replace(inState.Program, inState.EvSourceCode,
                inState.EvExprRange, inState.EvOptions.VarName, inState.StmtDivisors, inState.EvStmt,
                inState.EvRootScope, inState.EvExprVariables);
        }

        protected void Validate()
        {
            var edits = new List<SourceEdit>();
            edits.AddRange(inState.SourceEdits);
            edits.AddRange(assertEdits);
            validator = new EditsValidator(inState.FilePath, edits);
            validator.Execute();

            if (!validator.IsValid)
            {
                inState.Errors.Add("Error: Invalid selection");
            }
        }
    }
}
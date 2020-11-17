﻿using System;
using System.Collections.Generic;
using DafnyRefactor.Utils;

namespace DafnyRefactor.ExtractVariable
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

        public override void Handle(TState state)
        {
            if (state == null || state.EvExprRange == null || state.EvStmt == null || state.Program == null ||
                state.SourceCode == null || state.EvOptions == null || state.StmtDivisors == null ||
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
            (sourceEdits, assertEdits) = ExprOcurrencesReplacer.Replace(inState.Program, inState.SourceCode,
                inState.EvExprRange, inState.EvOptions.VarName, inState.StmtDivisors, inState.EvStmt,
                inState.EvRootScope, inState.EvExprVariables);
        }

        protected void Validate()
        {
            var edits = new List<SourceEdit>();
            edits.AddRange(inState.SourceEdits);
            edits.AddRange(assertEdits);
            var isValid = EditsValidator.IsValid(edits, inState.FilePath);

            if (!isValid)
            {
                inState.AddError(ExtractVariableErrorMsg.CantReplaceOccurrences());
            }
        }
    }
}
using System;
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
        private List<SourceEdit> _assertEdits;
        private TState _inState;
        private List<SourceEdit> _sourceEdits;

        public override void Handle(TState state)
        {
            if (state == null || state.EvExprRange == null || state.EvStmt == null || state.Program == null ||
                state.SourceCode == null || state.EvOptions == null || state.StmtDivisors == null ||
                state.SourceEdits == null || state.EvRootScope == null)
                throw new ArgumentNullException();

            _inState = state;

            Replace();
            Validate();
            if (state.Errors.Count > 0) return;

            state.SourceEdits.AddRange(_sourceEdits);
            base.Handle(state);
        }

        private void Replace()
        {
            (_sourceEdits, _assertEdits) = ExprOcurrencesReplacer.Replace(_inState.Program, _inState.SourceCode,
                _inState.EvExprRange, _inState.EvOptions.VarName, _inState.StmtDivisors, _inState.EvStmt,
                _inState.EvRootScope, _inState.EvExprVariables);
        }

        private void Validate()
        {
            var edits = new List<SourceEdit>();
            edits.AddRange(_inState.SourceEdits);
            edits.AddRange(_assertEdits);
            var isValid = EditsValidator.IsValid(edits, _inState.FilePath);

            if (!isValid)
            {
                _inState.AddError(ExtractVariableErrorMsg.CantReplaceOccurrences());
            }
        }
    }
}
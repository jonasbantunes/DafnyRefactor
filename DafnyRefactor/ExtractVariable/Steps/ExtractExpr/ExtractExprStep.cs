using System;
using DafnyRefactor.Utils;

namespace DafnyRefactor.ExtractVariable
{
    /// <summary>
    ///     A <c>RefactorStep</c> that extracts the selected expression from <c>state.EvStmt</c>.
    /// </summary>
    public class ExtractExprStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        private TState _inState;

        public override void Handle(TState state)
        {
            if (state == null || state.EvExprRange == null || state.EvStmt == null ||
                state.EvOptions == null || state.Program == null || state.SourceCode == null ||
                state.StmtDivisors == null || state.EvRootScope == null)
                throw new ArgumentNullException();

            _inState = state;

            ExtractExpr();
            ExtractVarsFromExpr();
            ValidateExpr();
            if (state.Errors.Count > 0) return;

            base.Handle(state);
        }

        private void ExtractExpr()
        {
            var exprStart = _inState.EvExprRange.start;
            var exprEnd = _inState.EvExprRange.end;
            var exprRaw = _inState.SourceCode.Substring(exprStart, exprEnd - exprStart).Trim();

            var varName = _inState.EvOptions.VarName;
            var editRaw = $"{Environment.NewLine}var {varName} := {exprRaw};";

            var divisorIndex =
                _inState.StmtDivisors.FindIndex(divisor => divisor > _inState.EvStmt.Tok.pos);
            var editPos = _inState.StmtDivisors[divisorIndex - 1] + 1;
            var edit = new SourceEdit(editPos, editRaw);
            _inState.SourceEdits.Add(edit);
        }

        private void ExtractVarsFromExpr()
        {
            var variables = ExprVarsExtractor.Extract(_inState.Program, _inState.EvExprRange, _inState.EvRootScope);
            _inState.EvExprVariables.AddRange(variables);
        }

        private void ValidateExpr()
        {
            var isValid = EditsValidator.IsValid(_inState.SourceEdits, _inState.FilePath);
            if (!isValid)
            {
                _inState.AddError(ExtractVariableErrorMsg.ExprInvalid());
            }
        }
    }
}
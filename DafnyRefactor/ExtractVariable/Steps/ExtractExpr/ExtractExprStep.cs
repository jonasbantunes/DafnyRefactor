using System;
using DafnyRefactor.Utils;

namespace DafnyRefactor.ExtractVariable
{
    /// <summary>
    ///     A <c>RefactorStep</c> that extracts the selected expression from <c>state.EvStmt</c>.
    /// </summary>
    public class ExtractExprStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        protected TState inState;

        public override void Handle(TState state)
        {
            if (state == null || state.EvExprRange == null || state.EvStmt == null ||
                state.EvOptions == null || state.Program == null || state.EvSourceCode == null ||
                state.StmtDivisors == null || state.EvRootScope == null)
                throw new ArgumentNullException();

            inState = state;

            ExtractExpr();
            ExtractVarsFromExpr();
            ValidateExpr();
            if (state.Errors.Count > 0) return;

            base.Handle(state);
        }

        protected void ExtractExpr()
        {
            var exprStart = inState.EvExprRange.start;
            var exprEnd = inState.EvExprRange.end;
            var exprRaw = inState.EvSourceCode.Substring(exprStart, exprEnd - exprStart).Trim();

            var varName = inState.EvOptions.VarName;
            var editRaw = $"\nvar {varName} := {exprRaw};";

            var divisorIndex =
                inState.StmtDivisors.FindIndex(divisor => divisor > inState.EvStmt.Tok.pos);
            var editPos = inState.StmtDivisors[divisorIndex - 1] + 1;
            var edit = new SourceEdit(editPos, editRaw);
            inState.SourceEdits.Add(edit);
        }

        protected void ExtractVarsFromExpr()
        {
            var variables = ExprVarsExtractor.Extract(inState.Program, inState.EvExprRange, inState.EvRootScope);
            inState.EvExprVariables.AddRange(variables);
        }

        protected void ValidateExpr()
        {
            var isValid = EditsValidator.IsValid(inState.SourceEdits, inState.FilePath);
            if (!isValid)
            {
                inState.Errors.Add("Error: Selected expression is invalid");
            }
        }
    }
}
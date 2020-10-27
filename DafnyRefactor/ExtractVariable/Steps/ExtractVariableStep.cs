using System;
using Microsoft.DafnyRefactor.ExtractVariable;
using Microsoft.DafnyRefactor.Utils;

namespace DafnyRefactor.ExtractVariable.Steps
{
    /// <summary>
    ///     A <c>RefactorStep</c> that extracts the selected expression from <c>state.ExtractStmt</c>.
    /// </summary>
    public class ExtractVariableStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        protected TState inState;

        public override void Handle(TState state)
        {
            if (state == null || state.ExprRange == null || state.ExtractStmt == null ||
                state.ExtractVariableOptions == null || state.Program == null || state.RawProgram == null ||
                state.StmtDivisors == null)
                throw new ArgumentNullException();

            inState = state;

            Extract();
            Validate();
            if (state.Errors.Count > 0) return;

            base.Handle(state);
        }

        protected void Extract()
        {
            var exprStart = inState.ExprRange.start;
            var exprEnd = inState.ExprRange.end;
            var exprRaw = inState.RawProgram.Substring(exprStart, exprEnd - exprStart).Trim();

            var varName = inState.ExtractVariableOptions.VarName;
            var editRaw = $"\nvar {varName} := {exprRaw};";

            var divisorIndex =
                inState.StmtDivisors.FindIndex(divisor => divisor >= inState.ExtractStmt.EndTok.pos);
            var editPos = inState.StmtDivisors[divisorIndex - 1] + 1;
            var edit = new SourceEdit(editPos, editRaw);
            inState.SourceEdits.Add(edit);
        }

        protected void Validate()
        {
            var validator = new EditsValidator(inState.FilePath, inState.SourceEdits);
            validator.Execute();
            if (!validator.IsValid)
            {
                inState.Errors.Add("Error: Selected expression is invalid");
            }
        }
    }
}
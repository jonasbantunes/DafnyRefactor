using System;
using Microsoft.DafnyRefactor.ExtractVariable;
using Microsoft.DafnyRefactor.Utils;

namespace DafnyRefactor.ExtractVariable.Steps
{
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

            ExtractVariable();
            Validate();
            if (state.Errors.Count > 0) return;

            base.Handle(state);
        }

        protected void ExtractVariable()
        {
            var startPos = inState.ExprRange.start;
            var endPos = inState.ExprRange.end;
            var rawExpr = inState.RawProgram.Substring(startPos, endPos - startPos);

            var varName = inState.ExtractVariableOptions.VarName;
            var extractedVarPrinted = $"\nvar {varName} := {rawExpr};";

            var divisorIndex =
                inState.StmtDivisors.FindIndex(divisor => divisor >= inState.ExtractStmt.EndTok.pos);
            var extractedPos = inState.StmtDivisors[divisorIndex - 1] + 1;
            var extracteVarEdit = new SourceEdit(extractedPos, extractedVarPrinted);
            inState.SourceEdits.Add(extracteVarEdit);
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
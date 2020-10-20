using System;
using Microsoft.DafnyRefactor.ExtractVariable;
using Microsoft.DafnyRefactor.Utils;

namespace DafnyRefactor.ExtractVariable.Steps
{
    public class ExtractVariableStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        protected TState internalState;

        public override void Handle(TState state)
        {
            if (state == null || state.ExprRange == null || state.ExtractStmt == null ||
                state.ExtractVariableOptions == null || state.Program == null || state.RawProgram == null ||
                state.StmtDivisors == null)
                throw new ArgumentNullException();
            internalState = state;

            ExtractVariable();
            var validator = new EditsValidator(state.Options.FilePath, state.SourceEdits);
            validator.Execute();
            if (!validator.IsValid)
            {
                state.Errors.Add("Error: Selected expression is invalid");
                return;
            }

            base.Handle(state);
        }

        protected void ExtractVariable()
        {
            var startPos = internalState.ExprRange.start;
            var endPos = internalState.ExprRange.end;
            var exprString = internalState.RawProgram.Substring(startPos, endPos - startPos);

            var varName = internalState.ExtractVariableOptions.VarName;
            var extractedVarPrinted = $"\nvar {varName} := {exprString};";

            var divisorIndex =
                internalState.StmtDivisors.FindIndex(divisor => divisor >= internalState.ExtractStmt.EndTok.pos);
            var extractedPos = internalState.StmtDivisors[divisorIndex - 1] + 1;
            var extracteVarEdit = new SourceEdit(extractedPos, extractedVarPrinted);
            internalState.SourceEdits.Add(extracteVarEdit);
            var removedVarEdit = new SourceEdit(startPos, endPos, varName);
            internalState.SourceEdits.Add(removedVarEdit);
        }
    }
}
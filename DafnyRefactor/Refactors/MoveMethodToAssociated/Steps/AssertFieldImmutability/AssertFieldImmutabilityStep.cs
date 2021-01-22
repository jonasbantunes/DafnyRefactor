using System;
using DafnyRefactor.Utils;

namespace DafnyRefactor.MoveMethodToAssociated
{
    class AssertFieldImmutabilityStep<TState> : RefactorStep<TState> where TState : IMoveToAssociatedState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.MtaOriginMethod == null || state.MtaTargetField == null ||
                state.Program == null || state.SourceCode == null ||
                state.StmtDivisors == null) throw new ArgumentNullException();

            var assertiveEdits = AssertivesGenerator.Generate(state.MtaTargetField, state.MtaOriginMethod,
                state.Program, state.SourceCode, state.StmtDivisors);
            var isValid = EditsValidator.IsValid(assertiveEdits, state.FilePath);
            if (!isValid)
            {
                state.AddError(MoveToAssociatedErrors.NotConstantBySolver());
                return;
            }

            base.Handle(state);
        }
    }
}
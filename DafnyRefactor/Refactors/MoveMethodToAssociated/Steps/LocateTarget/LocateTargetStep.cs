using System;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethodToAssociated
{
    public class LocateTargetStep<TState> : RefactorStep<TState> where TState : MoveToAssociatedState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null) throw new ArgumentNullException();

            var field = TargetLocator.Locate(state.Program, state.MtaTargetPos);
            if (field == null)
            {
                state.AddError(MoveToAssociatedErrors.NotFoundField());
                return;
            }

            if (!(field.Type is UserDefinedType userDefinedType))
            {
                state.AddError(MoveToAssociatedErrors.IsBuilIn());
                return;
            }

            if (!(userDefinedType.ResolvedClass is NonNullTypeDecl) && !(userDefinedType.ResolvedClass is ClassDecl))
            {
                state.AddError(MoveToAssociatedErrors.IsBuilIn());
                return;
            }

            if (userDefinedType.ResolvedClass is ClassDecl && !userDefinedType.IsNonNullRefType)
            {
                state.AddError(MoveToAssociatedErrors.IsNullable());
                return;
            }

            state.MtaTargetField = field;

            base.Handle(state);
        }
    }
}
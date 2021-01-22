using System;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethod
{
    public class LocateTargetStep<TState> : RefactorStep<TState> where TState : IMoveMethodState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null) throw new ArgumentNullException();

            var mvtParam = TargetLocator.Locate(state.Program, state.MvtUserTarget);
            if (mvtParam == null)
            {
                state.AddError(MoveMethodErrorMsg.NotFoundTarget());
                return;
            }

            if (!(mvtParam.Formal.Type is UserDefinedType userDefinedType))
            {
                state.AddError(MoveMethodErrorMsg.IsBuilIn());
                return;
            }

            if (!(userDefinedType.ResolvedClass is NonNullTypeDecl) && !(userDefinedType.ResolvedClass is ClassDecl))
            {
                state.AddError(MoveMethodErrorMsg.IsBuilIn());
                return;
            }

            if (userDefinedType.ResolvedClass is ClassDecl && !userDefinedType.IsNonNullRefType)
            {
                state.AddError(MoveMethodErrorMsg.IsNullable());
                return;
            }

            if (mvtParam.Method.EnclosingClass is DefaultClassDecl)
            {
                state.AddError(MoveMethodErrorMsg.DestClassDoesntExist(mvtParam.Method.Name));
                return;
            }

            state.MvtParam = mvtParam;

            base.Handle(state);
        }
    }
}
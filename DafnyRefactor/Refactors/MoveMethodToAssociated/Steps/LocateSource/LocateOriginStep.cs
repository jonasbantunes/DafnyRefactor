using System;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethodToAssociated
{
    public class LocateOriginStep<TState> : RefactorStep<TState> where TState : IMoveToAssociatedState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null) throw new ArgumentNullException();

            var mt = OriginLocator.Locate(state.Program, state.MtaOriginPos);
            if (mt == null)
            {
                state.AddError(MoveToAssociatedErrors.NotFoundMethod());
                return;
            }

            if (mt.EnclosingClass is DefaultClassDecl)
            {
                state.AddError(MoveToAssociatedErrors.MethodHasNoClass(mt.Name));
                return;
            }

            state.MtaOriginMethod = mt;

            base.Handle(state);
        }
    }
}
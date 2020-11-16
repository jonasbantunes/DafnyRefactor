using System;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.MoveMethod
{
    public class LocateTargetStep<TState> : RefactorStep<TState> where TState : IMoveMethodState
    {
        // TODO: too many IFs.
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null) throw new ArgumentNullException();

            var mvtParam = TargetLocator.Locate(state.Program, state.MvtUserTarget);
            if (mvtParam == null)
            {
                state.AddError("Error: can't locate target's parameter to be moved.");
                return;
            }

            if (!(mvtParam.Formal.Type is UserDefinedType userDefinedType))
            {
                state.AddError("Error: target type is built-in.");
                return;
            }

            if (!(userDefinedType.ResolvedClass is NonNullTypeDecl) && !(userDefinedType.ResolvedClass is ClassDecl))
            {
                state.AddError("Error: target type is built-in.");
                return;
            }

            if (userDefinedType.ResolvedClass is ClassDecl && !userDefinedType.IsNonNullRefType)
            {
                state.AddError("Error: target type is nullable.");
                return;
            }

            if (mvtParam.Method.EnclosingClass is DefaultClassDecl)
            {
                state.AddError($"Error: \"{mvtParam.Method.Name}()\" doesn't belong to a defined class.");
                return;
            }

            state.MvtParam = mvtParam;

            base.Handle(state);
        }
    }
}
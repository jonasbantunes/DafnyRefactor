using System;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethod
{
    public class CheckClassSignatureStep<TState> : RefactorStep<TState> where TState : IMoveMethodState
    {
        protected TState inState;

        public override void Handle(TState state)
        {
            if (state == null || state.MvtParam == null) throw new ArgumentNullException();

            inState = state;
            CheckSignature();

            base.Handle(state);
        }

        protected void CheckSignature()
        {
            if (!(inState.MvtParam.Formal.Type is UserDefinedType destType)) return;
            if (!(destType.ResolvedClass.ViewAsClass is ClassDecl destClass)) return;

            foreach (var member in destClass.Members)
            {
                if (!(member is Method method)) continue;
                if (method.Name != inState.MvtParam.Method.Name) continue;

                inState.AddError(
                    $"Error: method \"{inState.MvtParam.Method.Name}()\" already exists on class \"{destClass.Name}\".");
                return;
            }
        }
    }
}
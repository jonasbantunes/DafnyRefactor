using System;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethod
{
    public class CheckClassSignatureStep<TState> : RefactorStep<TState> where TState : IMoveMethodState
    {
        private TState _inState;

        public override void Handle(TState state)
        {
            if (state == null || state.MvtParam == null) throw new ArgumentNullException();

            _inState = state;
            CheckSignature();

            base.Handle(state);
        }

        private void CheckSignature()
        {
            if (!(_inState.MvtParam.Formal.Type is UserDefinedType destType)) return;
            if (!(destType.ResolvedClass.ViewAsClass is ClassDecl destClass)) return;

            foreach (var member in destClass.Members)
            {
                if (!(member is Method method)) continue;
                if (method.Name != _inState.MvtParam.Method.Name) continue;

                _inState.AddError(
                    $"Error: method \"{_inState.MvtParam.Method.Name}()\" already exists on class \"{destClass.Name}\".");
                return;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using DafnyRefactor.MoveMethod;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethodToAssociated
{
    public class FindMethodStep<TState> : RefactorStep<TState> where TState : IGetMoveToAssociatedState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null) throw new ArgumentNullException();

            var method = MethodFinder.Find(state.Program, state.GmtaMethodPos);
            if (method == null)
            {
                state.AddError("Error: can't find selected method.");
                return;
            }

            if (method.EnclosingClass is DefaultClassDecl)
            {
                state.AddError(MoveToAssociatedErrors.MethodHasNoClass(method.Name));
                return;
            }

            if (!(method.EnclosingClass is ClassDecl methodClass))
            {
                state.AddError("Error: method's class isn't a class.");
                return;
            }

            var paramsList = new List<string>();
            foreach (var member in methodClass.Members)
            {
                if (!(member is Field field))
                {
                    continue;
                }

                paramsList.Add(
                    $"{{ \"name\": \"{field.Name}\", \"type\": \"{field.Type}\", \"position\": \"{field.tok.line}:{field.tok.col}\" }}");
            }

            if (paramsList.Count < 1)
            {
                state.AddError("Method doesn't have any parameters.");
                return;
            }

            var json = $"[{string.Join(",", paramsList)}]";
            DafnyRefactorDriver.consoleOutput.Write(json);

            base.Handle(state);
        }
    }
}
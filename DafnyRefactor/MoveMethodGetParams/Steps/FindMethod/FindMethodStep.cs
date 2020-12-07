using System;
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethod
{
    public class FindMethodStep<TState> : RefactorStep<TState> where TState : IGetMoveMethodState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null) throw new ArgumentNullException();

            var method = MethodFinder.Find(state.Program, state.GmpMethodPos);
            if (method == null)
            {
                state.AddError("Error: can't find selected method.");
                return;
            }

            if (method.EnclosingClass is DefaultClassDecl)
            {
                state.AddError(MoveMethodErrorMsg.DestClassDoesntExist(method.Name));
                return;
            }

            var paramsList = new List<string>();
            foreach (var @in in method.Ins)
            {
                paramsList.Add(
                    $"{{ \"name\": \"{@in.Name}\", \"type\": \"{@in.Type}\", \"position\": \"{@in.tok.line}:{@in.tok.col}\" }}");
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
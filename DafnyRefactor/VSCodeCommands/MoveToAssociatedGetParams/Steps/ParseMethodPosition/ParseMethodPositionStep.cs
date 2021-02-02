using System;
using DafnyRefactor.Utils;

namespace DafnyRefactor.MoveMethodToAssociated
{
    public class ParseMethodPositionStep<TState> : RefactorStep<TState> where TState : IGetMoveToAssociatedState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Options == null || state.SourceCode == null)
                throw new ArgumentNullException();

            var positionRawSplitted = state.GmtaOptions.MethodPosition.Split(':');
            if (positionRawSplitted.Length != 2)
            {
                state.AddError(MoveToAssociatedErrors.WrongPositionSyntax());
                return;
            }

            var line = int.Parse(positionRawSplitted[0]);
            var col = int.Parse(positionRawSplitted[1]);

            var index = 0;
            if (line - 1 > 0)
            {
                index = state.SourceCode.IndexOfNth("\n", line - 1);
            }

            if (index == -1)
            {
                state.AddError(MoveToAssociatedErrors.InvalidPosition());
                return;
            }

            var position = index + col;
            state.GmtaMethodPos = position;

            base.Handle(state);
        }
    }
}
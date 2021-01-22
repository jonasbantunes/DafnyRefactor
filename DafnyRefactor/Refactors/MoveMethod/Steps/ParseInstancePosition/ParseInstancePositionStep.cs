using System;
using DafnyRefactor.Utils;

namespace DafnyRefactor.MoveMethod
{
    public class ParseInstancePositionStep<TState> : RefactorStep<TState> where TState : IMoveMethodState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Options == null || state.SourceCode == null)
                throw new ArgumentNullException();

            var positionRawSplitted = state.MvtOptions.InstancePosition.Split(':');
            if (positionRawSplitted.Length != 2)
            {
                state.AddError(MoveMethodErrorMsg.WrongPositionSyntax());
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
                state.AddError(MoveMethodErrorMsg.InvalidPosition());
                return;
            }

            var position = index + col;
            state.MvtUserTarget = position;

            base.Handle(state);
        }
    }
}
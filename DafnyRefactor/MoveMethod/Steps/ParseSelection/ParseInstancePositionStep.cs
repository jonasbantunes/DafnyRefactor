using System;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.MoveMethod
{
    public class ParseInstancePositionStep<TState> : RefactorStep<TState> where TState : IMoveMethodState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Options == null || state.MvtSourceCode == null)
                throw new ArgumentNullException();

            var positionRawSplitted = state.MvtOptions.InstancePosition.Split(':');
            if (positionRawSplitted.Length != 2)
            {
                state.Errors.Add("Error: Incorrect instance position syntax.");
                return;
            }

            var line = int.Parse(positionRawSplitted[0]);
            var col = int.Parse(positionRawSplitted[1]);

            var index = state.MvtSourceCode.IndexOfNth("\n", line - 1);
            if (index == -1)
            {
                state.Errors.Add("Error: Instance position is invalid");
                return;
            }

            var position = index + col;
            state.MvtUserInstance = position;

            base.Handle(state);
        }
    }
}
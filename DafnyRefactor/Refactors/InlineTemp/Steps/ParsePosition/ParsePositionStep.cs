using System;
using DafnyRefactor.Utils;

namespace DafnyRefactor.InlineTemp
{
    public class ParsePositionStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Options == null || state.SourceCode == null)
                throw new ArgumentNullException();

            var positionRawSplitted = state.InlineOptions.Position.Split(':');
            if (positionRawSplitted.Length != 2)
            {
                state.AddError(InlineTempErrorMsg.WrongPositionSyntax());
                return;
            }

            int line;
            int col;
            try
            {
                line = int.Parse(positionRawSplitted[0]);
                col = int.Parse(positionRawSplitted[1]);
            }
            catch (Exception)
            {
                state.AddError(InlineTempErrorMsg.InvalidPosition());
                return;
            }

            var index = 0;
            if (line - 1 > 0)
            {
                index = state.SourceCode.IndexOfNth("\n", line - 1);
            }

            if (index == -1)
            {
                state.AddError(InlineTempErrorMsg.InvalidPosition());
                return;
            }

            var position = index + col;
            state.IvrVariablePos = position;

            base.Handle(state);
        }
    }
}
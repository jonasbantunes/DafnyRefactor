using System;
using DafnyRefactor.Utils;

namespace DafnyRefactor.ExtractVariable
{
    /// <summary>
    ///     A <c>RefactorStep</c> that parses the selection range informed by user.
    /// </summary>
    public class ParseSelectionStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Options == null || state.SourceCode == null) throw new ArgumentNullException();

            var startRawSplitted = state.EvOptions.StartPosition.Split(':');
            var endRawSplitted = state.EvOptions.EndPosition.Split(':');
            if (startRawSplitted.Length != 2 || endRawSplitted.Length != 2)
            {
                state.AddError(ExtractVariableErrorMsg.WrongRangeSyntax());
                return;
            }

            var startLine = int.Parse(startRawSplitted[0]);
            var startCol = int.Parse(startRawSplitted[1]);
            var endLine = int.Parse(endRawSplitted[0]);
            var endCol = int.Parse(endRawSplitted[1]);

            var startIndex = 0;
            if (startLine - 1 > 0)
            {
                startIndex = state.SourceCode.IndexOfNth("\n", startLine - 1);
            }

            if (startIndex == -1)
            {
                state.AddError(ExtractVariableErrorMsg.RangeOutOfBounds());
                return;
            }

            var endIndex = 0;
            if (endLine - 1 > 0)
            {
                endIndex = state.SourceCode.IndexOfNth("\n", endLine - 1);
            }

            if (endIndex == -1)
            {
                state.AddError(ExtractVariableErrorMsg.RangeOutOfBounds());
                return;
            }

            var start = startIndex + startCol;
            var end = endIndex + endCol;
            if (start >= end)
            {
                state.AddError(ExtractVariableErrorMsg.WrongRange());
                return;
            }

            state.EvUserSelection = new Range(start, end);

            base.Handle(state);
        }
    }
}
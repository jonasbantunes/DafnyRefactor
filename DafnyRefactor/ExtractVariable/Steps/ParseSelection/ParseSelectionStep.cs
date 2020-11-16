using System;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    /// <summary>
    ///     A <c>RefactorStep</c> that parses the selection range informed by user.
    /// </summary>
    public class ParseSelectionStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Options == null || state.EvSourceCode == null) throw new ArgumentNullException();

            var startRawSplitted = state.EvOptions.StartPosition.Split(':');
            var endRawSplitted = state.EvOptions.EndPosition.Split(':');
            if (startRawSplitted.Length != 2 || endRawSplitted.Length != 2)
            {
                state.Errors.Add("Error: Incorrect selection range syntax.");
                return;
            }

            var startLine = int.Parse(startRawSplitted[0]);
            var startCol = int.Parse(startRawSplitted[1]);
            var endLine = int.Parse(endRawSplitted[0]);
            var endCol = int.Parse(endRawSplitted[1]);

            var startIndex = 0;
            if (startLine - 1 > 0)
            {
                startIndex = state.EvSourceCode.IndexOfNth("\n", startLine - 1);
            }

            if (startIndex == -1)
            {
                state.Errors.Add("Error: EvUserSelection is invalid");
                return;
            }

            var endIndex = 0;
            if (endLine - 1 > 0)
            {
                endIndex = state.EvSourceCode.IndexOfNth("\n", endLine - 1);
            }

            if (endIndex == -1)
            {
                state.Errors.Add("Error: EvUserSelection is invalid");
                return;
            }

            var start = startIndex + startCol;
            var end = endIndex + endCol;
            if (start >= end)
            {
                state.Errors.Add("Error: EvUserSelection is invalid");
                return;
            }

            state.EvUserSelection = new Range(start, end);

            base.Handle(state);
        }
    }
}
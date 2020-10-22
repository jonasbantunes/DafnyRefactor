using System;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public class ParseSelectionStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Options == null || state.RawProgram == null) throw new ArgumentNullException();

            var startRawSplitted = state.ExtractVariableOptions.StartPosition.Split(':');
            var endRawSplitted = state.ExtractVariableOptions.EndPosition.Split(':');
            if (startRawSplitted.Length != 2 || endRawSplitted.Length != 2)
            {
                state.Errors.Add("Error: Incorrect selection range syntax.");
                return;
            }

            var startLine = int.Parse(startRawSplitted[0]);
            var startCol = int.Parse(startRawSplitted[1]);
            var endLine = int.Parse(endRawSplitted[0]);
            var endCol = int.Parse(endRawSplitted[1]);

            var startIndex = state.RawProgram.IndexOfNth("\n", startLine - 1);
            if (startIndex == -1)
            {
                state.Errors.Add("Error: Selection is invalid");
                return;
            }

            var endIndex = state.RawProgram.IndexOfNth("\n", endLine - 1);
            if (endIndex == -1)
            {
                state.Errors.Add("Error: Selection is invalid");
                return;
            }

            var start = startIndex + startCol;
            var end = endIndex + endCol;
            if (start >= end)
            {
                state.Errors.Add("Error: Selection is invalid");
                return;
            }

            state.Selection = new Range(start, end);

            base.Handle(state);
        }
    }

    public static class ParseSelectionExtension
    {
        public static int IndexOfNth(this string str, string sub, int nth)
        {
            if (nth <= 0) throw new ArgumentException();

            var curIndex = -1;
            var count = 0;
            do
            {
                curIndex = str.IndexOf(sub, curIndex + 1, StringComparison.Ordinal);
                if (curIndex == -1) return -1;
                count++;
            } while (count < nth);

            return curIndex;
        }
    }
}
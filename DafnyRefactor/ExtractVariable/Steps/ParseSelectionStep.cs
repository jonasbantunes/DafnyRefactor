using System;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public class ParseSelectionStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        // TODO: Improve options validation
        // TODO: Improve source
        public override void Handle(TState state)
        {
            if (state == null || state.Options == null || state.RawProgram == null) throw new ArgumentNullException();

            var startLine = int.Parse(state.ExtractVariableOptions.StartPosition.Split(':')[0]);
            var startCol = int.Parse(state.ExtractVariableOptions.StartPosition.Split(':')[1]);
            var endLine = int.Parse(state.ExtractVariableOptions.EndPosition.Split(':')[0]);
            var endCol = int.Parse(state.ExtractVariableOptions.EndPosition.Split(':')[1]);
            var startIndex = state.RawProgram.IndexOfNth("\n", startLine);
            if (startIndex == -1)
            {
                state.Errors.Add("Error: Range is invalid");
                return;
            }

            var endIndex = state.RawProgram.IndexOfNth("\n", endLine);
            if (endIndex == -1)
            {
                state.Errors.Add("Error: Range is invalid");
                return;
            }

            var start = startIndex + startCol;
            var end = endIndex + endCol;
            state.Range = new Range(start, end);

            base.Handle(state);
        }
    }

    public static class ParseSelectionExtension
    {
        public static int IndexOfNth(this string str, string sub, int nth)
        {
            if (nth <= 0) throw new ArgumentException();

            var curIndex = 0;
            var count = 0;
            do
            {
                curIndex = str.IndexOf(sub, curIndex, StringComparison.Ordinal);
                if (curIndex == -1) return -1;
                count++;
            } while (count < nth);

            return curIndex;
        }
    }
}
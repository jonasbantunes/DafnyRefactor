using System;

namespace DafnyRefactor.Utils
{
    /// <summary>
    ///     Represents the lower and upper bounds of a sequence. <c>start</c> is always smaller or equal than <c>end</c>.
    /// </summary>
    public class Range
    {
        public readonly int end;
        public readonly int start;

        public Range(int start, int end)
        {
            if (start > end) throw new ArgumentException();

            this.start = start;
            this.end = end;
        }
    }
}
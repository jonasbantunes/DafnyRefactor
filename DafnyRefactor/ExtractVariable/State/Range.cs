namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public class Range
    {
        public readonly int end;
        public readonly int start;

        public Range(int start, int end)
        {
            this.start = start;
            this.end = end;
        }
    }
}
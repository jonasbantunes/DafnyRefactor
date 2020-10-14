namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public class Range
    {
        public readonly int start;
        public readonly int end;

        public Range(int start, int end)
        {
            this.start = start;
            this.end = end;
        }

        public Range(int pos)
        {
            start = pos;
            end = pos;
        }
    }
}
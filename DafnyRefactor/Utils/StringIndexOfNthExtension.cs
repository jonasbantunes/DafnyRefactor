using System;

namespace Microsoft.DafnyRefactor.Utils
{
    public static class StringIndexOfNthExtension
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
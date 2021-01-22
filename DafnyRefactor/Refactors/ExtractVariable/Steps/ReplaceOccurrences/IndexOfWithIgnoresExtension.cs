using DafnyRefactor.Utils;

namespace DafnyRefactor.ExtractVariable
{
    public static class IndexOfWithIgnoresExtension
    {
        public static Range IndexOfWithIgnores(this string str, string sub, int offset)
        {
            var strStart = offset;
            while (strStart < str.Length && str[strStart] == ' ')
            {
                strStart++;
            }

            if (strStart >= str.Length) return null;

            var subStart = 0;
            while (subStart < sub.Length && sub[subStart] == ' ')
            {
                subStart++;
            }

            if (subStart >= sub.Length) return null;

            var i = strStart;
            while (i < str.Length)
            {
                if (str[i] == sub[subStart])
                {
                    var strPos = i;
                    var subPos = subStart;

                    while (strPos < str.Length && subPos < sub.Length)
                    {
                        if (sub[subPos] == ' ')
                        {
                            subPos++;
                            continue;
                        }

                        if (str[strPos] == ' ')
                        {
                            strPos++;
                            continue;
                        }

                        if (str[strPos] != sub[subPos]) break;

                        strPos++;
                        subPos++;
                    }

                    if (subPos >= sub.Length)
                    {
                        return new Range(i, strPos);
                    }
                }

                i++;
            }

            return null;
        }
    }
}
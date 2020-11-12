using Microsoft.Dafny;

namespace Microsoft.DafnyRefactor.MoveMethod
{
    public interface IMvtParam
    {
        Method Method { get; }
        Formal Formal { get; }
        int ArgPos { get; }
    }

    public class MvtParam : IMvtParam
    {
        public MvtParam(Formal formal, int argPos, Method method)
        {
            Formal = formal;
            ArgPos = argPos;
            Method = method;
        }

        public Method Method { get; }
        public Formal Formal { get; }
        public int ArgPos { get; }
    }
}
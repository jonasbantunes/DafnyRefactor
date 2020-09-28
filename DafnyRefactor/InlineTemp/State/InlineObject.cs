using Microsoft.Dafny;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    public interface IInlineObject
    {
        string ObjPrinted { get; }
        string LhsPrinted { get; }
        Type ObjType { get; }
        Type MemberType { get; }
    }

    public class InlineObject : IInlineObject
    {
        public InlineObject(string objPrinted, string lhsPrinted, Type objType, Type memberType)
        {
            ObjPrinted = objPrinted;
            LhsPrinted = lhsPrinted;
            ObjType = objType;
            MemberType = memberType;
        }

        public InlineObject(string printed, Type objType, Type memberType)
        {
            LhsPrinted = printed;
            ObjPrinted = printed;
            ObjType = objType;
            MemberType = memberType;
        }

        public string ObjPrinted { get; }
        public string LhsPrinted { get; }
        public Type ObjType { get; }
        public Type MemberType { get; }
    }
}
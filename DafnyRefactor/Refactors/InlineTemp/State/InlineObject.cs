﻿using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     Represents an object from the expression to be refactored.
    ///     <para>
    ///         A special validation needed to be done with objects from expressions. This class contains
    ///         the neccessary date to verify if is possible to apply the "Inline Temp" refactor.
    ///     </para>
    /// </summary>
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
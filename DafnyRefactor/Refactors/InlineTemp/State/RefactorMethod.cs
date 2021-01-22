using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dafny;
using Type = Microsoft.Dafny.Type;

namespace DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     Represents the neccesary information of method, from a <c>Dafny.Program</c>, to apply the
    ///     "Inline Temp" refactor.
    /// </summary>
    public interface IRefactorMethod
    {
        Method Method { get; }
        List<IMethodArg> Args { get; }

        void InsertArg(string name, Type type, bool isInput, bool isOutput, bool canBeModified);
        IMethodArg LookupArg(string name);
    }

    public class RefactorMethod : IRefactorMethod
    {
        public RefactorMethod(Method method)
        {
            Method = method ?? throw new ArgumentNullException();
        }

        public Method Method { get; }
        public List<IMethodArg> Args { get; } = new List<IMethodArg>();

        public void InsertArg(string name, Type type, bool isInput, bool isOutput, bool canBeModified)
        {
            var arg = new MethodArg
            {
                Name = name,
                Type = type,
                IsInput = isInput,
                IsOutput = isOutput,
                CanBeModified = canBeModified
            };

            Args.Add(arg);
        }

        public IMethodArg LookupArg(string name)
        {
            return Args.FirstOrDefault(arg => arg.Name == name);
        }


        public override int GetHashCode()
        {
            return Method.GetHashCode();
        }
    }
}
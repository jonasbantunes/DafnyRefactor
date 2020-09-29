using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dafny;
using Type = Microsoft.Dafny.Type;

namespace DafnyRefactor.InlineTemp.State
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
        protected readonly Method method;
        protected List<IMethodArg> args = new List<IMethodArg>();

        public RefactorMethod(Method method)
        {
            this.method = method ?? throw new ArgumentNullException();
        }

        public Method Method => method;
        public List<IMethodArg> Args => args;

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

            args.Add(arg);
        }

        public IMethodArg LookupArg(string name)
        {
            return args.FirstOrDefault(arg => arg.Name == name);
        }


        public override int GetHashCode()
        {
            return method.GetHashCode();
        }
    }
}
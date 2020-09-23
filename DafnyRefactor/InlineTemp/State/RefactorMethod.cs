using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dafny;
using Type = Microsoft.Dafny.Type;

namespace DafnyRefactor.InlineTemp.State
{
    public interface IRefactorMethod
    {
        Method Method { get; }
        List<IMethodArg> Args { get; }

        void InsertArg(string name, Type type, bool isInput, bool isOutput, bool canBeModified);
        IMethodArg LookupArg(string name);
    }

    public class RefactorMethod : IRefactorMethod
    {
        protected List<IMethodArg> args = new List<IMethodArg>();
        protected readonly Method method;

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
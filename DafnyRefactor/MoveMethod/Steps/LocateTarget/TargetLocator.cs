using System;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.MoveMethod
{
    public class TargetLocator : DafnyVisitorWithNearests
    {
        protected Method foundMethod;
        protected Formal foundParam;
        protected int position;
        protected Program program;

        protected TargetLocator(Program program, int position)
        {
            if (program == null) throw new ArgumentNullException();

            this.program = program;
            this.position = position;
        }

        protected void Execute()
        {
            foundParam = null;
            Visit(program);
        }

        protected override void Visit(Method mt)
        {
            foreach (var @in in mt.Ins)
            {
                var inStart = @in.tok.pos;
                var inEnd = @in.tok.pos + @in.tok.val.Length;
                if (inStart > position || position >= inEnd) continue;

                foundParam = @in;
                foundMethod = mt;
            }
        }

        public static (Formal param, Method method) Locate(Program program, int position)
        {
            var locator = new TargetLocator(program, position);
            locator.Execute();
            return (locator.foundParam, locator.foundMethod);
        }
    }
}
using System;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethod
{
    public class TargetLocator : DafnyVisitorWithNearests
    {
        protected IMvtParam mvtParam;
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
            mvtParam = null;
            Visit(program);
        }

        protected override void Visit(Method mt)
        {
            for (var index = 0; index < mt.Ins.Count; index++)
            {
                var @in = mt.Ins[index];
                var inStart = @in.tok.pos;
                var inEnd = @in.tok.pos + @in.tok.val.Length;
                if (inStart > position || position >= inEnd) continue;

                mvtParam = new MvtParam(@in, index, mt);
            }
        }

        public static IMvtParam Locate(Program program, int position)
        {
            var locator = new TargetLocator(program, position);
            locator.Execute();
            return locator.mvtParam;
        }
    }
}
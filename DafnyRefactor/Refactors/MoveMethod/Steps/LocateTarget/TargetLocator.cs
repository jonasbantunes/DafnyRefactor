using System;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethod
{
    public class TargetLocator : DafnyVisitorWithNearests
    {
        private readonly int _position;
        private readonly Program _program;
        private IMvtParam _mvtParam;

        private TargetLocator(Program program, int position)
        {
            _program = program ?? throw new ArgumentNullException();
            _position = position;
        }

        private void Execute()
        {
            _mvtParam = null;
            Visit(_program);
        }

        protected override void Visit(Method mt)
        {
            for (var index = 0; index < mt.Ins.Count; index++)
            {
                var @in = mt.Ins[index];
                var inStart = @in.tok.pos;
                var inEnd = @in.tok.pos + @in.tok.val.Length;
                if (inStart > _position || _position >= inEnd) continue;

                _mvtParam = new MvtParam(@in, index, mt);
            }
        }

        public static IMvtParam Locate(Program program, int position)
        {
            var locator = new TargetLocator(program, position);
            locator.Execute();
            return locator._mvtParam;
        }
    }
}
using System;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethodToAssociated
{
    public class OriginLocator : DafnyVisitor
    {
        private readonly int _position;
        private readonly Program _program;
        private Method _method;

        private OriginLocator(Program program, int position)
        {
            _program = program ?? throw new ArgumentNullException();
            _position = position;
        }

        private void Execute()
        {
            _method = null;
            Visit(_program);
        }

        protected override void Visit(Method mt)
        {
            var start = mt.tok.pos;
            var end = mt.tok.pos + mt.tok.val.Length;

            if (start > _position || _position >= end) return;
            _method = mt;
        }

        public static Method Locate(Program program, int position)
        {
            var locate = new OriginLocator(program, position);
            locate.Execute();
            return locate._method;
        }
    }
}
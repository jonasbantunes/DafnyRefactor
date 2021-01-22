using System;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethod
{
    public class MethodFinder : DafnyVisitor
    {
        private readonly int _position;
        private readonly Program _program;
        private Method _method;

        private MethodFinder(Program program, int position)
        {
            _program = program ?? throw new ArgumentNullException();
            _position = position;
        }

        private void Execute()
        {
            Visit(_program);
        }

        protected override void Visit(Method mt)
        {
            if (mt == null) return;

            var start = mt.tok.pos;
            var end = mt.tok.pos + mt.tok.val.Length;
            if (start <= _position && _position < end)
            {
                _method = mt;
            }

            base.Visit(mt);
        }

        public static Method Find(Program program, int position)
        {
            var finder = new MethodFinder(program, position);
            finder.Execute();
            return finder._method;
        }
    }
}
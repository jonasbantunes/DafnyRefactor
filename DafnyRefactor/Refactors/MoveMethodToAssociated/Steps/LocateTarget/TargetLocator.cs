using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethodToAssociated
{
    public class TargetLocator : DafnyVisitor
    {
        private readonly int _position;
        private readonly Program _program;
        private Field _field;

        private TargetLocator(Program program, int position)
        {
            _program = program;
            _position = position;
        }

        private void Execute()
        {
            _field = null;
            Visit(_program);
        }

        protected override void Visit(Field field)
        {
            var start = field.tok.pos;
            var end = field.tok.pos + field.tok.val.Length;

            if (start > _position || _position >= end) return;
            _field = field;
        }

        public static Field Locate(Program program, int position)
        {
            var locator = new TargetLocator(program, position);
            locator.Execute();
            return locator._field;
        }
    }
}
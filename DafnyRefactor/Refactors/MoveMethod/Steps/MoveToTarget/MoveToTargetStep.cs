using System;
using System.Linq;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethod
{
    public class MoveToTargetStep<TState> : RefactorStep<TState> where TState : IMoveMethodState
    {
        private TState _inState;

        public override void Handle(TState state)
        {
            if (state == null || state.MvtParam == null || state.SourceCode == null)
                throw new ArgumentNullException();

            _inState = state;
            MoveMethod();
            Clean();

            base.Handle(state);
        }


        private void MoveMethod()
        {
            var replacedEdits =
                MethodReplacer.Replace(_inState.MvtParam.Method, _inState.MvtParam.Formal, _inState.SourceCode);

            var tokStart = _inState.MvtParam.Method.tok.pos;
            var start = _inState.SourceCode.LastIndexOf("method", tokStart, StringComparison.Ordinal);
            var end = _inState.MvtParam.Method.BodyEndTok.pos + 1;

            var paramType = (UserDefinedType) _inState.MvtParam.Formal.Type;
            var pos = paramType.ResolvedClass.ViewAsClass.BodyEndTok.pos;

            var methodCode = _inState.SourceCode.Substring(start, end - start);
            var methodEdits = replacedEdits
                .Select(ed => new SourceEdit(ed.startPos - start, ed.endPos - start, ed.content)).ToList();
            var replacedCode = SourceEditor.Edit(methodCode, methodEdits);
            var content = $"{Environment.NewLine}{replacedCode}{Environment.NewLine}";

            var edit = new SourceEdit(pos, content);
            _inState.SourceEdits.Add(edit);
        }

        private void Clean()
        {
            var tokStart = _inState.MvtParam.Method.tok.pos;
            var start = _inState.SourceCode.LastIndexOf("method", tokStart, StringComparison.Ordinal);
            var end = _inState.MvtParam.Method.BodyEndTok.pos + 1;

            var edit = new SourceEdit(start, end, "");
            _inState.SourceEdits.Add(edit);
        }
    }
}
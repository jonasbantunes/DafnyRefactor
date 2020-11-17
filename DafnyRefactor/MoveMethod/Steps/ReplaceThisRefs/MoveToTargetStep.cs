using System;
using System.Linq;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethod
{
    public class MoveToTargetStep<TState> : RefactorStep<TState> where TState : IMoveMethodState
    {
        protected TState inState;

        public override void Handle(TState state)
        {
            if (state == null || state.MvtParam == null || state.MvtSourceCode == null)
                throw new ArgumentNullException();

            inState = state;
            MoveMethod();
            Clean();

            base.Handle(state);
        }


        protected void MoveMethod()
        {
            var replacedEdits =
                MethodReplacer.Replace(inState.MvtParam.Method, inState.MvtParam.Formal, inState.MvtSourceCode);

            var tokStart = inState.MvtParam.Method.tok.pos;
            var start = inState.MvtSourceCode.LastIndexOf("method", tokStart, StringComparison.Ordinal);
            var end = inState.MvtParam.Method.BodyEndTok.pos + 1;

            var paramType = (UserDefinedType) inState.MvtParam.Formal.Type;
            var pos = paramType.ResolvedClass.ViewAsClass.BodyEndTok.pos;

            var methodCode = inState.MvtSourceCode.Substring(start, end - start);
            var methodEdits = replacedEdits
                .Select(ed => new SourceEdit(ed.startPos - start, ed.endPos - start, ed.content)).ToList();
            var replacedCode = SourceEditor.Edit(methodCode, methodEdits);
            var content = $"{Environment.NewLine}{replacedCode}{Environment.NewLine}";

            var edit = new SourceEdit(pos, content);
            inState.SourceEdits.Add(edit);
        }

        protected void Clean()
        {
            var tokStart = inState.MvtParam.Method.tok.pos;
            var start = inState.MvtSourceCode.LastIndexOf("method", tokStart, StringComparison.Ordinal);
            var end = inState.MvtParam.Method.BodyEndTok.pos + 1;

            var edit = new SourceEdit(start, end, "");
            inState.SourceEdits.Add(edit);
        }
    }
}
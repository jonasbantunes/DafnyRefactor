using System;
using System.Linq;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.MoveMethod
{
    public class MoveToTargetStep<TState> : RefactorStep<TState> where TState : IMoveMethodState
    {
        protected TState inState;

        public override void Handle(TState state)
        {
            if (state == null || state.MvtMethod == null || state.MvtParam == null || state.MvtSourceCode == null)
                throw new ArgumentNullException();

            inState = state;
            MoveMethod();
            Clean();

            base.Handle(state);
        }


        protected void MoveMethod()
        {
            var replacedEdits = MethodReplacer.Replace(inState.MvtMethod, inState.MvtParam, inState.MvtSourceCode);

            var tokStart = inState.MvtMethod.tok.pos;
            var start = inState.MvtSourceCode.LastIndexOf("method", tokStart, StringComparison.Ordinal);
            var end = inState.MvtMethod.BodyEndTok.pos + 1;

            var paramType = (UserDefinedType) inState.MvtParam.Type;
            var pos = paramType.ResolvedClass.ViewAsClass.BodyEndTok.pos;

            var methodCode = inState.MvtSourceCode.Substring(start, end - start);
            var methodEdits = replacedEdits
                .Select(ed => new SourceEdit(ed.startPos - start, ed.endPos - start, ed.content)).ToList();
            var replacedCode = SourceEditor.Edit(methodCode, methodEdits);
            var content = $"\n{replacedCode}\n";

            var edit = new SourceEdit(pos, content);
            inState.SourceEdits.Add(edit);
        }

        protected void Clean()
        {
            var tokStart = inState.MvtMethod.tok.pos;
            var start = inState.MvtSourceCode.LastIndexOf("method", tokStart, StringComparison.Ordinal);
            var end = inState.MvtMethod.BodyEndTok.pos + 1;

            var edit = new SourceEdit(start, end, "");
            inState.SourceEdits.Add(edit);
        }
    }
}
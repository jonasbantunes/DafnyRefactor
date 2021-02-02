using System;
using System.Collections.Generic;
using System.Linq;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethodToAssociated
{
    public class MethodUpdater : DafnyVisitor
    {
        private readonly Field _field;
        private readonly Method _method;
        private readonly string _sourceCode;
        private List<SourceEdit> _edits;
        private string _newParamName;

        private MethodUpdater(Method method, Field field, string sourceCode)
        {
            _method = method;
            _field = field;
            _sourceCode = sourceCode;
        }

        private void Execute()
        {
            _edits = new List<SourceEdit>();
            AddParam();
            Visit(_method);
            TransferMethod();
            RemoveOriginalMethod();
        }

        private void AddParam()
        {
            var className = _method.EnclosingClass.Name;
            _newParamName = char.ToLower(className[0]) + className.Substring(1);
            var newParamRaw = $"{_newParamName}: {className}";
            if (_method.Ins.Count > 0)
            {
                newParamRaw += ", ";
            }

            var pos = _sourceCode.IndexOf("(", _method.tok.pos, StringComparison.Ordinal) + 1;
            var edit = new SourceEdit(pos, newParamRaw);
            _edits.Add(edit);

            foreach (var modifies in _method.Mod.Expressions)
            {
                Visit(modifies.E);
            }

            foreach (var requires in _method.Req)
            {
                Visit(requires.E);
            }

            foreach (var ensures in _method.Ens)
            {
                Visit(ensures.E);
            }
        }

        private void TransferMethod()
        {
            var tokStart = _method.tok.pos;
            var start = _sourceCode.LastIndexOf("method", tokStart, StringComparison.Ordinal);
            var end = _method.BodyEndTok.pos + 1;

            var paramType = (UserDefinedType) _field.Type;
            var pos = paramType.ResolvedClass.ViewAsClass.BodyEndTok.pos;

            var methodCode = _sourceCode.Substring(start, end - start);
            var methodEdits = _edits
                .Select(ed => new SourceEdit(ed.startPos - start, ed.endPos - start, ed.content)).ToList();
            var replacedCode = SourceEditor.Edit(methodCode, methodEdits);
            var content = $"{Environment.NewLine}{replacedCode}{Environment.NewLine}";

            var edit = new SourceEdit(pos, content);
            _edits.Clear();
            _edits.Add(edit);
        }

        private void RemoveOriginalMethod()
        {
            var tokStart = _method.tok.pos;
            var start = _sourceCode.LastIndexOf("method", tokStart, StringComparison.Ordinal);
            var end = _method.BodyEndTok.pos + 1;

            var edit = new SourceEdit(start, end, "");
            _edits.Add(edit);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            if (nameSeg.Name == _field.Name)
            {
                var start = nameSeg.tok.pos;
                var end = nameSeg.tok.pos + nameSeg.tok.val.Length;
                var edit = new SourceEdit(start, end, "this");
                _edits.Add(edit);
            }

            base.Visit(nameSeg);
        }

        protected override void Visit(MemberSelectExpr memberSelectExpr)
        {
            if (memberSelectExpr.Obj is ThisExpr thisExpr && !thisExpr.IsImplicit &&
                memberSelectExpr.Member is Field field && field.Name == _field.Name)
            {
                var start = thisExpr.tok.pos;
                var end = thisExpr.tok.pos + thisExpr.tok.val.Length + 1 + field.Name.Length;
                var edit = new SourceEdit(start, end, "this");
                _edits.Add(edit);

                return;
            }

            base.Visit(memberSelectExpr);
        }

        protected override void Visit(ThisExpr thisExpr)
        {
            // TODO: Analyse a better approach to constructors
            if (thisExpr.tok.val == "new") return;
            if (thisExpr.tok.val == _field.Name) return;

            // TODO: Improve source structure
            var start = thisExpr.tok.pos;
            var end = thisExpr.tok.pos + thisExpr.tok.val.Length;
            var paramName = thisExpr.tok.val == "this" ? _newParamName : $"{_newParamName}.{thisExpr.tok.val}";
            var edit = new SourceEdit(start, end, paramName);
            _edits.Add(edit);
        }

        public static List<SourceEdit> Update(Method method, Field field, string sourceCode)
        {
            var updater = new MethodUpdater(method, field, sourceCode);
            updater.Execute();
            return updater._edits;
        }
    }
}
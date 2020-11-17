using System;
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethod
{
    public class MethodReplacer : DafnyVisitorWithNearests
    {
        private readonly Method _method;
        private readonly Formal _param;
        private readonly string _sourceCode;
        private List<SourceEdit> _edits;
        private string _newParamName;

        private MethodReplacer(Method method, Formal param, string sourceCode)
        {
            if (method == null || param == null || sourceCode == null) throw new ArgumentNullException();

            _method = method;
            _param = param;
            _sourceCode = sourceCode;
        }

        private void Execute()
        {
            _edits = new List<SourceEdit>();
            ReplaceParam();
            Visit(_method);
        }

        private void ReplaceParam()
        {
            foreach (var @in in _method.Ins)
            {
                if (@in.Name != _param.Name) continue;

                var start = @in.tok.pos;
                var end = start;
                while (end <= _sourceCode.Length && _sourceCode[end] != ',' && _sourceCode[end] != ')') end++;

                var className = _method.EnclosingClass.Name;
                _newParamName = char.ToLower(className[0]) + className.Substring(1);
                var newParamRaw = $"{_newParamName}: {className}";
                var edit = new SourceEdit(start, end, newParamRaw);
                _edits.Add(edit);
            }

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

        protected override void Visit(NameSegment nameSeg)
        {
            if (nameSeg.Name == _param.Name)
            {
                var start = nameSeg.tok.pos;
                var end = nameSeg.tok.pos + nameSeg.tok.val.Length;
                var edit = new SourceEdit(start, end, "this");
                _edits.Add(edit);
            }

            base.Visit(nameSeg);
        }

        protected override void Visit(ThisExpr thisExpr)
        {
            // TODO: Analyse a better approach to constructors
            if (thisExpr.tok.val == "new") return;

            var start = thisExpr.tok.pos;
            var end = thisExpr.tok.pos + thisExpr.tok.val.Length;
            var paramName = thisExpr.tok.val == "this" ? _newParamName : $"{_newParamName}.{thisExpr.tok.val}";
            var edit = new SourceEdit(start, end, paramName);
            _edits.Add(edit);
        }

        public static List<SourceEdit> Replace(Method method, Formal param, string sourceCode)
        {
            var replacer = new MethodReplacer(method, param, sourceCode);
            replacer.Execute();
            return replacer._edits;
        }
    }
}
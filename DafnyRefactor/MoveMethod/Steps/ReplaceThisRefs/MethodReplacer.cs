using System;
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethod
{
    public class MethodReplacer : DafnyVisitorWithNearests
    {
        protected List<SourceEdit> edits;
        protected Method method;
        protected string newParamName;
        protected Formal param;
        protected string sourceCode;

        protected MethodReplacer(Method method, Formal param, string sourceCode)
        {
            if (method == null || param == null || sourceCode == null) throw new ArgumentNullException();

            this.method = method;
            this.param = param;
            this.sourceCode = sourceCode;
        }

        protected void Execute()
        {
            edits = new List<SourceEdit>();
            ReplaceParam();
            Visit(method);
        }

        protected void ReplaceParam()
        {
            foreach (var @in in method.Ins)
            {
                if (@in.Name != param.Name) continue;

                var start = @in.tok.pos;
                var end = start;
                while (end <= sourceCode.Length && sourceCode[end] != ',' && sourceCode[end] != ')') end++;

                var className = method.EnclosingClass.Name;
                newParamName = char.ToLower(className[0]) + className.Substring(1);
                var newParamRaw = $"{newParamName}: {className}";
                var edit = new SourceEdit(start, end, newParamRaw);
                edits.Add(edit);
            }

            foreach (var modifies in method.Mod.Expressions)
            {
                Visit(modifies.E);
            }

            foreach (var requires in method.Req)
            {
                Visit(requires.E);
            }

            foreach (var ensures in method.Ens)
            {
                Visit(ensures.E);
            }
        }

        protected override void Visit(NameSegment nameSeg)
        {
            if (nameSeg.Name == param.Name)
            {
                var start = nameSeg.tok.pos;
                var end = nameSeg.tok.pos + nameSeg.tok.val.Length;
                var edit = new SourceEdit(start, end, "this");
                edits.Add(edit);
            }

            base.Visit(nameSeg);
        }

        protected override void Visit(ThisExpr thisExpr)
        {
            // TODO: Analyse a better approach to constructors
            if (thisExpr.tok.val == "new") return;

            var start = thisExpr.tok.pos;
            var end = thisExpr.tok.pos + thisExpr.tok.val.Length;
            var paramName = thisExpr.tok.val == "this" ? newParamName : $"{newParamName}.{thisExpr.tok.val}";
            var edit = new SourceEdit(start, end, paramName);
            edits.Add(edit);
        }

        public static List<SourceEdit> Replace(Method method, Formal param, string sourceCode)
        {
            var replacer = new MethodReplacer(method, param, sourceCode);
            replacer.Execute();
            return replacer.edits;
        }
    }
}
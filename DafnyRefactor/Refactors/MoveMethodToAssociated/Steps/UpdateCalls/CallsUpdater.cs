using System;
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethodToAssociated
{
    public class CallsUpdater : DafnyVisitor
    {
        private readonly Field _field;
        private readonly Method _method;
        private readonly Program _program;
        private readonly string _sourceCode;
        private List<SourceEdit> _edits;

        private CallsUpdater(Program program, string sourceCode, Method method, Field field)
        {
            _program = program;
            _method = method;
            _field = field;
            _sourceCode = sourceCode;
        }

        private void Execute()
        {
            _edits = new List<SourceEdit>();
            Visit(_program);
        }

        protected override void Visit(CallStmt callStmt)
        {
            if (!callStmt.Method.Equals(_method)) return;

            var receiverRange = ExprRangeFinder.Find(callStmt.Receiver, _sourceCode);
            var receiverRaw = receiverRange != null
                ? _sourceCode.Substring(receiverRange.start, receiverRange.end - receiverRange.start)
                : "this";

            var argPos = _sourceCode.IndexOf("(", callStmt.Tok.pos, StringComparison.Ordinal) + 1;
            var argRaw = receiverRaw == "this" ? _field.Name : $"{receiverRaw}.{_field.Name}";

            if (_method.Ins.Count > 0)
            {
                receiverRaw += ", ";
            }

            _edits.Add(new SourceEdit(argPos, receiverRaw));
            _edits.Add(receiverRange != null
                ? new SourceEdit(receiverRange.start, receiverRange.end, $"{argRaw}")
                : new SourceEdit(callStmt.Receiver.tok.pos, $"{argRaw}."));

            base.Visit(callStmt);
        }

        public static List<SourceEdit> Update(Program program, string sourceCode, Method method, Field field)
        {
            var updater = new CallsUpdater(program, sourceCode, method, field);
            updater.Execute();
            return updater._edits;
        }
    }
}
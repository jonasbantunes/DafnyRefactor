using System;
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethod
{
    public class CallsUpdater : DafnyVisitor
    {
        private readonly IMvtParam _mvtParam;
        private readonly Program _program;
        private readonly string _sourceCode;
        private List<SourceEdit> _edits;

        private CallsUpdater(Program program, string sourceCode, IMvtParam mvtParam)
        {
            if (program == null || sourceCode == null || mvtParam == null) throw new ArgumentNullException();

            _program = program;
            _sourceCode = sourceCode;
            _mvtParam = mvtParam;
        }

        private void Execute()
        {
            _edits = new List<SourceEdit>();
            Visit(_program);
        }

        protected override void Visit(Method mt)
        {
            if (mt == _mvtParam.Method) return;

            base.Visit(mt);
        }

        protected override void Visit(CallStmt callStmt)
        {
            if (!callStmt.Method.Equals(_mvtParam.Method)) return;
            if (callStmt.Args.Count < _mvtParam.ArgPos + 1) return;

            var receiverRange = ExprRangeFinder.Find(callStmt.Receiver, _sourceCode);
            var receiverRaw = receiverRange != null
                ? _sourceCode.Substring(receiverRange.start, receiverRange.end - receiverRange.start)
                : "this";

            var argRange = ExprRangeFinder.Find(callStmt.Args[_mvtParam.ArgPos], _sourceCode);
            var argRaw = _sourceCode.Substring(argRange.start, argRange.end - argRange.start);
            _edits.Add(new SourceEdit(argRange.start, argRange.end, receiverRaw));

            _edits.Add(receiverRange != null
                ? new SourceEdit(receiverRange.start, receiverRange.end, $"({argRaw})")
                : new SourceEdit(callStmt.Receiver.tok.pos, $"({argRaw})."));

            base.Visit(callStmt);
        }

        public static List<SourceEdit> Update(Program program, string sourceCode, IMvtParam mvtParam)
        {
            var updater = new CallsUpdater(program, sourceCode, mvtParam);
            updater.Execute();
            return updater._edits;
        }
    }
}
using System;
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethod
{
    public class CallsUpdater : DafnyVisitor
    {
        protected List<SourceEdit> edits;
        protected IMvtParam mvtParam;
        protected Program program;
        protected string sourceCode;

        protected CallsUpdater(Program program, string sourceCode, IMvtParam mvtParam)
        {
            if (program == null || sourceCode == null || mvtParam == null) throw new ArgumentNullException();

            this.program = program;
            this.sourceCode = sourceCode;
            this.mvtParam = mvtParam;
        }

        protected void Execute()
        {
            edits = new List<SourceEdit>();
            Visit(program);
        }

        protected override void Visit(Method mt)
        {
            if (mt == mvtParam.Method) return;

            base.Visit(mt);
        }

        protected override void Visit(CallStmt callStmt)
        {
            if (!callStmt.Method.Equals(mvtParam.Method)) return;
            if (callStmt.Args.Count < mvtParam.ArgPos + 1) return;

            var receiverRange = ExprRangeFinder.Find(callStmt.Receiver, sourceCode);
            var receiverRaw = receiverRange != null
                ? sourceCode.Substring(receiverRange.start, receiverRange.end - receiverRange.start)
                : "this";

            var argRange = ExprRangeFinder.Find(callStmt.Args[mvtParam.ArgPos], sourceCode);
            var argRaw = sourceCode.Substring(argRange.start, argRange.end - argRange.start);
            edits.Add(new SourceEdit(argRange.start, argRange.end, receiverRaw));

            edits.Add(receiverRange != null
                ? new SourceEdit(receiverRange.start, receiverRange.end, $"({argRaw})")
                : new SourceEdit(callStmt.Receiver.tok.pos, $"({argRaw})."));

            base.Visit(callStmt);
        }

        public static List<SourceEdit> Update(Program program, string sourceCode, IMvtParam mvtParam)
        {
            var updater = new CallsUpdater(program, sourceCode, mvtParam);
            updater.Execute();
            return updater.edits;
        }
    }
}
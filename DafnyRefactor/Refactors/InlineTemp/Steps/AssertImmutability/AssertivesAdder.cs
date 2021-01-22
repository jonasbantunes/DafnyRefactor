using System;
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     A utility that adds asserves before usage of an <c>InlineObject</c>.
    /// </summary>
    public class AssertivesAdder : DafnyVisitorWithNearests
    {
        private readonly IInlineVariable _inlineVariable;
        private readonly Program _program;
        private readonly IInlineScope _rootScope;
        private readonly List<int> _stmtDivisors;
        private List<SourceEdit> _edits;

        private AssertivesAdder(Program program, List<int> stmtDivisors, IInlineScope rootScope,
            IInlineVariable inlineVariable)
        {
            if (rootScope == null || program == null || stmtDivisors == null || inlineVariable == null)
                throw new ArgumentNullException();

            _program = program;
            _stmtDivisors = stmtDivisors;
            _rootScope = rootScope;
            _inlineVariable = inlineVariable;
        }

        private IInlineScope CurScope => _rootScope.FindInlineScope(nearestScopeToken.GetHashCode());

        private void Execute()
        {
            _edits = new List<SourceEdit>();
            Visit(_program);
        }

        protected override void Visit(AssignStmt assignStmt)
        {
            if (nearestStmt.Tok.pos < _inlineVariable.InitStmt.EndTok.pos) return;
            if (!(assignStmt.Lhs is MemberSelectExpr memberSelectExpr)) return;

            var findIndex = _stmtDivisors.FindIndex(divisor => divisor >= nearestStmt.EndTok.pos);
            if (findIndex <= 1) return;

            if (CurScope == null) return;
            foreach (var inlineObject in CurScope.GetInlineObjects())
            {
                if (inlineObject.MemberType == null) continue;
                if (!memberSelectExpr.Type.Equals(inlineObject.MemberType)) continue;
                var obj = memberSelectExpr.Obj;
                if (!obj.Type.Equals(inlineObject.ObjType)) continue;

                var assertStmtExpr =
                    $"{Environment.NewLine} assert {Printer.ExprToString(obj)} != {inlineObject.LhsPrinted};";
                var pos = _stmtDivisors[findIndex - 1] + 1;
                var edit = new SourceEdit(pos, assertStmtExpr);
                _edits.Add(edit);
            }
        }

        protected override void Visit(CallStmt callStmt)
        {
            var divisorIndex = _stmtDivisors.FindIndex(divisor => divisor >= nearestStmt.EndTok.pos);
            if (divisorIndex <= 1) return;

            var method = CurScope?.LookupMethod(callStmt.Method.GetHashCode());
            if (method == null) return;

            for (var i = 0; i < callStmt.Args.Count; i++)
            {
                var arg = callStmt.Args[i];
                var methodArg = method.Args[i];

                if (!methodArg.CanBeModified) continue;
                foreach (var inlineObject in CurScope.GetInlineObjects())
                {
                    if (!methodArg.Type.Equals(inlineObject.ObjType)) continue;

                    var argPrinted = Printer.ExprToString(arg);
                    var assertStmtExpr = $"{Environment.NewLine} assert {argPrinted} != {inlineObject.ObjPrinted};";

                    var pos = _stmtDivisors[divisorIndex - 1] + 1;
                    var edit = new SourceEdit(pos, assertStmtExpr);
                    _edits.Add(edit);
                }
            }
        }

        public static List<SourceEdit> GetAssertives(Program program, List<int> stmtDivisors, IInlineScope rootScope,
            IInlineVariable inlineVariable)
        {
            var adder = new AssertivesAdder(program, stmtDivisors, rootScope, inlineVariable);
            adder.Execute();
            return adder._edits;
        }
    }
}
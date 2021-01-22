using System;
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     A utility that adds assertives on each <c>InlineVariable</c> usage.
    /// </summary>
    public class AssertivesSimplifiedAdder : DafnyVisitorWithNearests
    {
        private readonly IInlineVariable _inlineVariable;
        private readonly Program _program;
        private readonly IRefactorScope _rootTable;
        private readonly List<int> _stmtDivisors;
        private List<SourceEdit> _edits;

        private AssertivesSimplifiedAdder(Program program, IRefactorScope rootTable, IInlineVariable inlineVariable,
            List<int> stmtDivisors)
        {
            if (program == null || rootTable == null || inlineVariable == null || stmtDivisors == null)
                throw new ArgumentNullException();

            _program = program;
            _inlineVariable = inlineVariable;
            _rootTable = rootTable;
            _stmtDivisors = stmtDivisors;
        }

        private IRefactorScope CurScope => _rootTable.FindScope(nearestScopeToken.GetHashCode());

        private void Execute()
        {
            _edits = new List<SourceEdit>();

            var varName = _inlineVariable.Name;
            var varExprPrinted = Printer.ExprToString(_inlineVariable.Expr);
            var ghostStmt = $"{Environment.NewLine} ghost var {varName}___RefactorGhostExpr := {varExprPrinted};";

            var pos = _inlineVariable.InitStmt.EndTok.pos + 1;
            var edit = new SourceEdit(pos, ghostStmt);
            _edits.Add(edit);

            Visit(_program);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            var divisorIndex = _stmtDivisors.FindIndex(divisor => divisor >= nearestStmt.EndTok.pos);
            if (divisorIndex <= 1) return;

            var variable = CurScope.LookupVariable(nameSeg.Name);
            if (variable == null || !variable.Equals(_inlineVariable)) return;

            var varName = _inlineVariable.Name;
            var varExprPrinted = Printer.ExprToString(_inlineVariable.Expr);
            var assertStmtExpr = $"{Environment.NewLine} assert {varName}___RefactorGhostExpr == {varExprPrinted};";

            var pos = _stmtDivisors[divisorIndex - 1] + 1;
            var edit = new SourceEdit(pos, assertStmtExpr);
            _edits.Add(edit);
        }

        public static List<SourceEdit> GetAssertives(Program program, IRefactorScope rootTable,
            IInlineVariable inlineVariable, List<int> stmtDivisors)
        {
            var adder = new AssertivesSimplifiedAdder(program, rootTable, inlineVariable, stmtDivisors);
            adder.Execute();
            return adder._edits;
        }
    }
}
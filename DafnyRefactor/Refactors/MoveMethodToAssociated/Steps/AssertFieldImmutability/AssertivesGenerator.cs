using System;
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethodToAssociated
{
    public class AssertivesGenerator : DafnyVisitorWithNearests
    {
        private readonly Field _field;
        private readonly Method _method;
        private readonly Program _program;
        private readonly string _sourceCode;
        private readonly List<int> _stmtDivisors;
        private List<SourceEdit> _edits;

        private AssertivesGenerator(Field field, Method method, Program program, string sourceCode,
            List<int> stmtDivisors)
        {
            _program = program;
            _field = field;
            _method = method;
            _sourceCode = sourceCode;
            _stmtDivisors = stmtDivisors;
        }

        private void Execute()
        {
            _edits = new List<SourceEdit>();

            var varName = _field.Name;
            var ghostStmt = $"{Environment.NewLine} ghost var {varName}___RefactorGhostExpr := {varName};";

            var pos = _method.BodyStartTok.pos + 1;
            var edit = new SourceEdit(pos, ghostStmt);
            _edits.Add(edit);

            Visit(_program);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            if (_method.BodyStartTok.pos > nameSeg.tok.pos || nameSeg.tok.pos >= _method.BodyEndTok.pos) return;
            if (nameSeg.Name != _field.Name) return;

            var divisorIndex = _stmtDivisors.FindIndex(divisor => divisor >= nearestStmt.EndTok.pos);
            if (divisorIndex <= 1) return;

            var varName = _field.Name;
            var assertStmtExpr = $"{Environment.NewLine} assert {varName}___RefactorGhostExpr == {varName};";

            var pos = _stmtDivisors[divisorIndex - 1] + 1;
            var edit = new SourceEdit(pos, assertStmtExpr);
            _edits.Add(edit);
        }

        public static List<SourceEdit> Generate(Field field, Method method, Program program, string sourceCode,
            List<int> stmtDivisors)
        {
            var generator = new AssertivesGenerator(field, method, program, sourceCode, stmtDivisors);
            generator.Execute();
            return generator._edits;
        }
    }
}
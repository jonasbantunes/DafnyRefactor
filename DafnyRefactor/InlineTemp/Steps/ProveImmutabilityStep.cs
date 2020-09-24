using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    public class ProveImmutabilityStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Errors == null || state.FilePath == null || state.InlineOptions == null ||
                state.Program == null || state.StmtDivisors == null ||
                state.RootScope == null) throw new ArgumentNullException();

            var addAssertives = new AddAssertivesVisitor(state.Program, state.RootScope, state.InlineVariable,
                state.StmtDivisors);
            addAssertives.Execute();

            var checker = new InlineImmutabilityCheck(state.FilePath, addAssertives.Edits);
            checker.Execute();

            if (!checker.IsConstant)
            {
                state.AddError(
                    $"Error: variable {state.InlineVariable.Name} located on {state.InlineOptions.VarLine}:{state.InlineOptions.VarColumn} is not constant according with theorem prover.");
                return;
            }

            base.Handle(state);
        }
    }

    internal class AddAssertivesVisitor : DafnyVisitorWithNearests
    {
        protected IInlineVariable inlineVariable;
        protected Program program;
        protected IRefactorScope rootTable;
        protected List<int> stmtDivisors;


        public AddAssertivesVisitor(Program program, IRefactorScope rootTable, IInlineVariable inlineVariable,
            List<int> stmtDivisors)
        {
            if (program == null || rootTable == null || inlineVariable == null || stmtDivisors == null)
                throw new ArgumentNullException();

            this.program = program;
            this.inlineVariable = inlineVariable;
            this.rootTable = rootTable;
            this.stmtDivisors = stmtDivisors;
        }

        protected IRefactorScope CurScope => rootTable.FindScope(nearestScopeToken.GetHashCode());
        public List<SourceEdit> Edits { get; protected set; }

        public virtual void Execute()
        {
            Edits = new List<SourceEdit>();

            var varName = inlineVariable.Name;
            var varExprPrinted = Printer.ExprToString(inlineVariable.Expr);
            var ghostStmt = $"\n ghost var {varName}___RefactorGhostExpr := {varExprPrinted};\n";

            var pos = inlineVariable.InitStmt.EndTok.pos + 1;
            var edit = new SourceEdit(pos, ghostStmt);
            Edits.Add(edit);

            Visit(program);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            var divisorIndex = stmtDivisors.FindIndex(divisor => divisor >= nearestStmt.EndTok.pos);
            if (divisorIndex <= 1) return;

            var variable = CurScope.LookupVariable(nameSeg.Name);
            if (variable == null || !variable.Equals(inlineVariable)) return;

            var varName = inlineVariable.Name;
            var varExprPrinted = Printer.ExprToString(inlineVariable.Expr);
            var assertStmtExpr = $"\n assert {varName}___RefactorGhostExpr == {varExprPrinted};\n";

            var pos = stmtDivisors[divisorIndex - 1] + 1;
            var edit = new SourceEdit(pos, assertStmtExpr);
            Edits.Add(edit);
        }
    }

    internal class InlineImmutabilityCheck
    {
        protected List<SourceEdit> edits;
        protected string filePath;

        public InlineImmutabilityCheck(string filePath, List<SourceEdit> edits)
        {
            if (filePath == null || edits == null) throw new ArgumentNullException();

            this.filePath = filePath;
            this.edits = edits;
        }

        public bool IsConstant { get; protected set; }

        public void Execute()
        {
            var source = File.ReadAllText(filePath);
            var sourceEditor = new SourceEditor(source, edits);
            sourceEditor.Apply();

            var tempPath = Path.GetTempPath() + Guid.NewGuid() + ".dfy";
            File.WriteAllText(tempPath, sourceEditor.Source);

            var args = new[] {tempPath, "/compile:0"};
            var res = DafnyDriver.Main(args);
            IsConstant = res == 0;
            File.Delete(tempPath);
        }
    }
}
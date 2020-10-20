using System;
using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     A <c>RefactorStep</c> that verifies if all usages of a <c>InlineVariable</c> are constant.
    ///     ///
    ///     <para>
    ///         This is a simplified approach of <c>AssertImmutabilityStep</c>.
    ///     </para>
    /// </summary>
    public class AssertImmutabilitySimplifiedStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Errors == null || state.FilePath == null || state.InlineOptions == null ||
                state.Program == null || state.StmtDivisors == null ||
                state.RootScope == null) throw new ArgumentNullException();

            var addAssertives = new AssertivesSimplifiedAdder(state.Program, state.RootScope, state.InlineVariable,
                state.StmtDivisors);
            addAssertives.Execute();

            var checker = new EditsValidator(state.FilePath, addAssertives.Edits);
            checker.Execute();

            if (!checker.IsValid)
            {
                state.AddError(
                    $"Error: variable {state.InlineVariable.Name} located on {state.InlineOptions.VarLine}:{state.InlineOptions.VarColumn} is not constant according with theorem prover.");
                return;
            }

            base.Handle(state);
        }
    }

    /// <summary>
    ///     A utility that adds assertives on each <c>InlineVariable</c> usage.
    /// </summary>
    internal class AssertivesSimplifiedAdder : DafnyVisitorWithNearests
    {
        protected IInlineVariable inlineVariable;
        protected Program program;
        protected IRefactorScope rootTable;
        protected List<int> stmtDivisors;

        public AssertivesSimplifiedAdder(Program program, IRefactorScope rootTable, IInlineVariable inlineVariable,
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
            var ghostStmt = $"\n ghost var {varName}___RefactorGhostExpr := {varExprPrinted};";

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
            var assertStmtExpr = $"\n assert {varName}___RefactorGhostExpr == {varExprPrinted};";

            var pos = stmtDivisors[divisorIndex - 1] + 1;
            var edit = new SourceEdit(pos, assertStmtExpr);
            Edits.Add(edit);
        }
    }
}
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

        public List<SourceEdit> Edits { get; protected set; }

        public virtual void Execute()
        {
            Edits = new List<SourceEdit>();
            var ghostStmtExpr =
                $"\n ghost var {inlineVariable.Name}___RefactorGhostExpr := {Printer.ExprToString(inlineVariable.Expr)};\n";
            Edits.Add(new SourceEdit(inlineVariable.InitStmt.EndTok.pos + 1, ghostStmtExpr));
            Visit(program);
        }

        protected override void Visit(UpdateStmt up)
        {
            if (up == null) throw new ArgumentNullException();

            Traverse(up.Rhss);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            if (nameSeg == null) throw new ArgumentNullException();

            // TODO: Avoid this repetition on source code
            var curTable = rootTable.FindScope(nearestBlockStmt.Tok.GetHashCode());
            if (curTable.LookupVariable(nameSeg.Name)?.GetHashCode() == inlineVariable.GetHashCode())
            {
                var findIndex = stmtDivisors.FindIndex(divisor => divisor >= nearestStmt.EndTok.pos);
                if (findIndex <= 1) return;

                var assertStmtExpr =
                    $"\n assert {inlineVariable.Name}___RefactorGhostExpr == {Printer.ExprToString(inlineVariable.Expr)};\n";
                Edits.Add(new SourceEdit(stmtDivisors[findIndex - 1] + 1, assertStmtExpr));
            }

            base.Visit(nameSeg);
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

            var res = DafnyDriver.Main(new[] {tempPath, "/compile:0"});
            IsConstant = res == 0;
            File.Delete(tempPath);
        }
    }
}
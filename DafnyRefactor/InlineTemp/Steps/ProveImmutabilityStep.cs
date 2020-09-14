using System;
using System.Collections.Generic;
using System.IO;
using DafnyRefactor.InlineTemp.InlineTable;
using DafnyRefactor.Utils;
using DafnyRefactor.Utils.DafnyVisitor;
using DafnyRefactor.Utils.SourceEdit;
using DafnyRefactor.Utils.SymbolTable;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp.Steps
{
    public class ProveImmutabilityStep : RefactorStep<InlineState>
    {
        public override void Handle(InlineState state)
        {
            var addAssertives = new AddAssertivesVisitor(state.program, state.symbolTable, state.inlineSymbol);
            addAssertives.Execute();

            var checker = new InlineImmutabilityCheck(state.FilePath, addAssertives.Edits);
            checker.Execute();
            if (!checker.IsConstant)
            {
                state.errors.Add(
                    $"Error: variable {state.inlineSymbol.Name} located on {state.inlineOptions.VarLine}:{state.inlineOptions.VarColumn} is not constant according with theorem prover.");
                return;
            }

            base.Handle(state);
        }
    }

    internal class InlineImmutabilityCheck
    {
        protected string filePath;
        protected List<SourceEdit> edits;
        public bool IsConstant { get; protected set; }

        public InlineImmutabilityCheck(string filePath, List<SourceEdit> edits)
        {
            this.filePath = filePath;
            this.edits = edits;
        }

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

    internal class AddAssertivesVisitor : DafnyWithTableVisitor<InlineSymbol>
    {
        protected Statement nearestStmt;
        protected InlineSymbol inlineSymbol;
        public List<SourceEdit> Edits { get; protected set; }


        public AddAssertivesVisitor(Program program, SymbolTable<InlineSymbol> rootTable, InlineSymbol inlineSymbol) :
            base(program, rootTable)
        {
            this.inlineSymbol = inlineSymbol;
        }

        public override void Execute()
        {
            Edits = new List<SourceEdit>();
            var ghostStmtExpr =
                $"\n ghost var {inlineSymbol.Name}___RefactorGhostExpr := {Printer.ExprToString(inlineSymbol.expr)};\n";
            Edits.Add(new SourceEdit(inlineSymbol.initStmt.EndTok.pos + 1, ghostStmtExpr));
            base.Execute();
        }

        protected override void Visit(Statement stmt)
        {
            var oldNearestStmt = nearestStmt;
            nearestStmt = stmt;
            base.Visit(stmt);
            nearestStmt = oldNearestStmt;
        }

        protected override void Visit(UpdateStmt up)
        {
            Traverse(up.Rhss);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            if (curTable.LookupSymbol(nameSeg.Name).GetHashCode() == inlineSymbol.GetHashCode())
            {
                var assertStmtExpr =
                    $"\n assert {inlineSymbol.Name}___RefactorGhostExpr == {Printer.ExprToString(inlineSymbol.expr)};\n";
                Edits.Add(new SourceEdit(nearestStmt.Tok.pos, assertStmtExpr));
            }

            base.Visit(nameSeg);
        }
    }
}
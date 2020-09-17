﻿using System;
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
        protected List<SourceEdit> edits;
        protected string filePath;

        public InlineImmutabilityCheck(string filePath, List<SourceEdit> edits)
        {
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

    internal class AddAssertivesVisitor : DafnyVisitor
    {
        protected IInlineSymbol inlineSymbol;
        protected Statement nearestStmt;
        protected ISymbolTable rootTable;


        public AddAssertivesVisitor(Program program, ISymbolTable rootTable, IInlineSymbol inlineSymbol) : base(program)
        {
            this.inlineSymbol = inlineSymbol;
            this.rootTable = rootTable;
        }

        public List<SourceEdit> Edits { get; protected set; }

        public override void Execute()
        {
            Edits = new List<SourceEdit>();
            var ghostStmtExpr =
                $"\n ghost var {inlineSymbol.Name}___RefactorGhostExpr := {Printer.ExprToString(inlineSymbol.Expr)};\n";
            Edits.Add(new SourceEdit(inlineSymbol.InitStmt.EndTok.pos + 1, ghostStmtExpr));
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
            // TODO: Avoid this repetition on source code
            var curTable = rootTable.FindTable(nearestBlockStmt.Tok.GetHashCode());
            if (curTable.LookupSymbol(nameSeg.Name).GetHashCode() == inlineSymbol.GetHashCode())
            {
                var assertStmtExpr =
                    $"\n assert {inlineSymbol.Name}___RefactorGhostExpr == {Printer.ExprToString(inlineSymbol.Expr)};\n";
                Edits.Add(new SourceEdit(nearestStmt.Tok.pos, assertStmtExpr));
            }

            base.Visit(nameSeg);
        }
    }
}
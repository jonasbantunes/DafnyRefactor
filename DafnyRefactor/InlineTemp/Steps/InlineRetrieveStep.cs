﻿using System.Collections.Generic;
using DafnyRefactor.InlineTemp.InlineTable;
using DafnyRefactor.Utils;
using DafnyRefactor.Utils.DafnyVisitor;
using DafnyRefactor.Utils.SourceEdit;
using DafnyRefactor.Utils.SymbolTable;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp.Steps
{
    public class InlineRetrieveStep : RefactorStep<InlineState>
    {
        public override void Handle(InlineState state)
        {
            var visitor = new InlineRetrieveVisitor(state.program, state.symbolTable, state.inlineSymbol);
            visitor.Execute();
            var inVar = visitor.InlineVar;
            if (inVar.expr == null)
            {
                state.errors.Add(
                    $"Error: variable {inVar.Name} located on {state.inlineOptions.VarLine}:{state.inlineOptions.VarColumn} is never initialized.");
                return;
            }

            if (inVar.isUpdated)
            {
                state.errors.Add(
                    $"Error: variable {inVar.Name} located on {state.inlineOptions.VarLine}:{state.inlineOptions.VarColumn} is not constant.");
                return;
            }

            state.inlineSymbol = inVar;
            state.immutabilitySourceEdits = visitor.Edits;

            base.Handle(state);
        }
    }

    internal class InlineRetrieveVisitor : DafnyWithTableVisitor<InlineSymbol>
    {
        protected InlineSymbol declaration;
        public InlineSymbol InlineVar { get; protected set; }
        public List<SourceEdit> Edits { get; protected set; }

        public InlineRetrieveVisitor(Program program, SymbolTable<InlineSymbol> rootTable, InlineSymbol declaration) :
            base(
                program, rootTable)
        {
            this.declaration = declaration;
        }

        public override void Execute()
        {
            curTable = rootTable;
            // TODO: Check if a full copy is neccessary
            InlineVar = new InlineSymbol(declaration.localVariable, declaration.varDeclStmt);
            Edits = new List<SourceEdit>();

            base.Execute();
        }

        protected override void Visit(VarDeclStmt vds)
        {
            if (!(vds.Update is UpdateStmt up)) return;
            for (var i = 0; i < up.Lhss.Count; i++)
            {
                if (up.Lhss[i] is AutoGhostIdentifierExpr agie && agie.Name == InlineVar.Name &&
                    curTable.LookupSymbol(agie.Name).GetHashCode() == InlineVar.GetHashCode())
                {
                    var erhs = (ExprRhs) up.Rhss[i];
                    InlineVar.expr = erhs.Expr;
                    InlineVar.initStmt = up;

                    var ghostStmt = $"\n ghost var {InlineVar.Name}___RefactorGhost := {InlineVar.Name};\n";
                    var ghostStmtExpr =
                        $"\n ghost var {InlineVar.Name}___RefactorGhostExpr := {Printer.ExprToString(InlineVar.expr)};\n";
                    var assertStmt = $"\n assert {InlineVar.Name}___RefactorGhost == {InlineVar.Name};\n";
                    var assertStmtExpr =
                        $"\n assert {InlineVar.Name}___RefactorGhostExpr == {Printer.ExprToString(InlineVar.expr)};\n";
                    Edits.Add(new SourceEdit(vds.EndTok.pos + 1, ghostStmt));
                    Edits.Add(new SourceEdit(vds.EndTok.pos + 1, ghostStmtExpr));
                    Edits.Add(new SourceEdit(curTable.blockStmt.EndTok.pos, assertStmt));
                    Edits.Add(new SourceEdit(curTable.blockStmt.EndTok.pos, assertStmtExpr));
                }
            }
        }

        protected override void Visit(UpdateStmt up)
        {
            for (var i = 0; i < up.Lhss.Count; i++)
            {
                if (up.Lhss[i] is NameSegment nm && nm.Name == InlineVar.Name &&
                    curTable.LookupSymbol(nm.Name).GetHashCode() == InlineVar.GetHashCode())
                {
                    if (InlineVar.expr == null && up.Rhss[i] is ExprRhs erhs)
                    {
                        InlineVar.expr = erhs.Expr;
                        InlineVar.initStmt = up;

                        var ghostStmt = $"\n ghost var {InlineVar.Name}___RefactorGhost := {InlineVar.Name};\n";
                        var ghostStmtExpr =
                            $"\n ghost var {InlineVar.Name}___RefactorGhostExpr := {Printer.ExprToString(InlineVar.expr)};\n";
                        var assertStmt = $"\n assert {InlineVar.Name}___RefactorGhost == {InlineVar.Name};\n";
                        var assertStmtExpr =
                            $"\n assert {InlineVar.Name}___RefactorGhostExpr == {Printer.ExprToString(InlineVar.expr)};\n";
                        Edits.Add(new SourceEdit(up.EndTok.pos + 1, ghostStmt));
                        Edits.Add(new SourceEdit(up.EndTok.pos + 1, ghostStmtExpr));
                        Edits.Add(new SourceEdit(curTable.blockStmt.EndTok.pos, assertStmt));
                        Edits.Add(new SourceEdit(curTable.blockStmt.EndTok.pos, assertStmtExpr));
                    }
                    else
                    {
                        InlineVar.isUpdated = true;
                    }
                }
            }
        }
    }
}
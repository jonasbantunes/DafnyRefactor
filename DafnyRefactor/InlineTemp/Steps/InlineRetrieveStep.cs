using System.Collections.Generic;
using DafnyRefactor.Utils.DafnyVisitor;
using DafnyRefactor.Utils.SourceEdit;
using DafnyRefactor.Utils.SymbolTable;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp.Steps
{
    public class InlineRetrieveStep : DafnyWithTableVisitor
    {
        protected SymbolTableDeclaration declaration;
        public InlineVariable InlineVar { get; protected set; }
        public List<SourceEdit> Edits { get; protected set; }

        public InlineRetrieveStep(Program program, SymbolTable rootTable, SymbolTableDeclaration declaration) : base(
            program, rootTable)
        {
            this.declaration = declaration;
        }

        public override void Execute()
        {
            curTable = rootTable;
            InlineVar = new InlineVariable(declaration);
            Edits = new List<SourceEdit>();

            base.Execute();
        }

        protected override void Visit(VarDeclStmt vds)
        {
            if (!(vds.Update is UpdateStmt up)) return;
            for (var i = 0; i < up.Lhss.Count; i++)
            {
                if (up.Lhss[i] is AutoGhostIdentifierExpr agie && agie.Name == InlineVar.Name &&
                    curTable.LookupDeclaration(agie.Name).GetHashCode() == InlineVar.TableDeclaration.GetHashCode())
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
                    curTable.LookupDeclaration(nm.Name).GetHashCode() == InlineVar.TableDeclaration.GetHashCode())
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
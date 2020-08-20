using System.Collections.Generic;

namespace Microsoft.Dafny
{
    public class InlineRetrieveStep : DafnyWithTableVisitor
    {
        protected SymbolTableDeclaration declaration;
        public InlineVariable InlineVar { get; protected set; }
        public List<SourceEdit> Edits { get; protected set; }

        public InlineRetrieveStep(Program program, SymbolTable rootTable, SymbolTableDeclaration declaration) : base(program, rootTable)
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

        protected override VarDeclStmt Visit(VarDeclStmt vds)
        {
            if (vds.Update is UpdateStmt up)
            {
                for (int i = 0; i < up.Lhss.Count; i++)
                {
                    if (up.Lhss[i] is AutoGhostIdentifierExpr agie && agie.Name == InlineVar.Name && curTable.LookupDeclaration(agie.Name).GetHashCode() == InlineVar.TableDeclaration.GetHashCode())
                    {
                        ExprRhs erhs = (ExprRhs)up.Rhss[i];
                        InlineVar.expr = erhs.Expr;

                        string ghostStmt = $"\n ghost var {$"{InlineVar.Name}___RefactorGhost"} := {InlineVar.Name};\n";
                        string ghostStmtExpr = $"\n ghost var {$"{InlineVar.Name}___RefactorGhostExpr"} := {Printer.ExprToString(InlineVar.expr)};\n";
                        string assertStmt = $"\n assert {$"{InlineVar.Name}___RefactorGhost"} == {InlineVar.Name};\n";
                        string assertStmtExpr = $"\n assert {$"{InlineVar.Name}___RefactorGhostExpr"} == {Printer.ExprToString(InlineVar.expr)};\n";
                        Edits.Add(new SourceEdit(vds.EndTok.pos + 1, ghostStmt));
                        Edits.Add(new SourceEdit(vds.EndTok.pos + 1, ghostStmtExpr));
                        Edits.Add(new SourceEdit(curTable.blockStmt.EndTok.pos, assertStmt));
                        Edits.Add(new SourceEdit(curTable.blockStmt.EndTok.pos, assertStmtExpr));
                    }
                }
            }

            return vds;
        }

        protected override UpdateStmt Visit(UpdateStmt up)
        {
            for (int i = 0; i < up.Lhss.Count; i++)
            {
                if (up.Lhss[i] is NameSegment nm && nm.Name == InlineVar.Name && curTable.LookupDeclaration(nm.Name).GetHashCode() == InlineVar.TableDeclaration.GetHashCode())
                {
                    if (InlineVar.expr == null && up.Rhss[i] is ExprRhs erhs)
                    {
                        InlineVar.expr = erhs.Expr;

                        string ghostStmt = $"\n ghost var {$"{InlineVar.Name}___RefactorGhost"} := {InlineVar.Name};\n";
                        string ghostStmtExpr = $"\n ghost var {$"{InlineVar.Name}___RefactorGhostExpr"} := {Printer.ExprToString(InlineVar.expr)};\n";
                        string assertStmt = $"\n assert {$"{InlineVar.Name}___RefactorGhost"} == {InlineVar.Name};\n";
                        string assertStmtExpr = $"\n assert {$"{InlineVar.Name}___RefactorGhostExpr"} == {Printer.ExprToString(InlineVar.expr)};\n";
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

            return up;
        }
    }
}

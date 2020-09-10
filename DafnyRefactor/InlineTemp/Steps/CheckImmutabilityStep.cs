using DafnyRefactor.InlineTemp.InlineTable;
using DafnyRefactor.Utils;
using DafnyRefactor.Utils.DafnyVisitor;
using DafnyRefactor.Utils.SymbolTable;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp.Steps
{
    public class CheckImmutabilityStep : RefactorStep<InlineState>
    {
        public override void Handle(InlineState state)
        {
            var visitor = new InlineRetrieveVisitor(state.program, state.symbolTable);
            visitor.Execute();
            if (state.inlineSymbol.expr == null)
            {
                state.errors.Add(
                    $"Error: variable {state.inlineSymbol.Name} located on {state.inlineOptions.VarLine}:{state.inlineOptions.VarColumn} is never initialized.");
                return;
            }

            if (state.inlineSymbol.isUpdated)
            {
                state.errors.Add(
                    $"Error: variable {state.inlineSymbol.Name} located on {state.inlineOptions.VarLine}:{state.inlineOptions.VarColumn} is not constant.");
                return;
            }

            base.Handle(state);
        }
    }

    internal class InlineRetrieveVisitor : DafnyWithTableVisitor<InlineSymbol>
    {
        public InlineRetrieveVisitor(Program program, SymbolTable<InlineSymbol> rootTable) :
            base(
                program, rootTable)
        {
        }

        protected override void Visit(VarDeclStmt vds)
        {
            if (!(vds.Update is UpdateStmt up)) return;
            for (var i = 0; i < up.Lhss.Count; i++)
            {
                if (!(up.Lhss[i] is AutoGhostIdentifierExpr agie)) continue;
                var symbol = curTable.LookupSymbol(agie.Name);
                if (symbol == null) continue;
                var erhs = (ExprRhs) up.Rhss[i];
                symbol.expr = erhs.Expr;
                symbol.initStmt = up;
            }
        }

        protected override void Visit(UpdateStmt up)
        {
            for (var i = 0; i < up.Lhss.Count; i++)
            {
                if (!(up.Lhss[i] is NameSegment nm)) continue;
                var symbol = curTable.LookupSymbol(nm.Name);
                if (symbol.expr == null && up.Rhss[i] is ExprRhs erhs)
                {
                    symbol.expr = erhs.Expr;
                    symbol.initStmt = up;
                }
                else
                {
                    symbol.isUpdated = true;
                }
            }
        }
    }
}
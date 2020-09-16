using DafnyRefactor.InlineTemp.InlineTable;
using DafnyRefactor.Utils;
using DafnyRefactor.Utils.DafnyVisitor;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp.Steps
{
    public class CheckImmutabilityStep : RefactorStep<InlineState>
    {
        public override void Handle(InlineState state)
        {
            var visitor = new InlineRetrieveVisitor(state.program, state.symbolTable);
            visitor.Execute();
            if (state.inlineSymbol.Expr == null)
            {
                state.errors.Add(
                    $"Error: variable {state.inlineSymbol.Name} located on {state.inlineOptions.VarLine}:{state.inlineOptions.VarColumn} is never initialized.");
                return;
            }

            if (state.inlineSymbol.IsUpdated)
            {
                state.errors.Add(
                    $"Error: variable {state.inlineSymbol.Name} located on {state.inlineOptions.VarLine}:{state.inlineOptions.VarColumn} is not constant.");
                return;
            }

            base.Handle(state);
        }
    }

    internal class InlineRetrieveVisitor : DafnyVisitor
    {
        protected IInlineTable rootTable;

        public InlineRetrieveVisitor(Program program, IInlineTable rootTable) : base(program)
        {
            this.rootTable = rootTable;
        }

        protected override void Visit(VarDeclStmt vds)
        {
            if (!(vds.Update is UpdateStmt up)) return;
            for (var i = 0; i < up.Lhss.Count; i++)
            {
                if (!(up.Lhss[i] is AutoGhostIdentifierExpr agie)) continue;
                // TODO: Avoid this repetition on source code
                var curTable = rootTable.FindInlineTable(nearestBlockStmt.Tok.GetHashCode());
                var symbol = curTable.LookupInlineSymbol(agie.Name);
                if (symbol == null) continue;
                var erhs = (ExprRhs) up.Rhss[i];
                symbol.Expr = erhs.Expr;
                symbol.InitStmt = up;
            }
        }

        protected override void Visit(UpdateStmt up)
        {
            for (var i = 0; i < up.Lhss.Count; i++)
            {
                if (!(up.Lhss[i] is NameSegment nm)) continue;
                var curTable = rootTable.FindInlineTable(nearestBlockStmt.Tok.GetHashCode());
                var symbol = curTable.LookupInlineSymbol(nm.Name);
                if (symbol.Expr == null && up.Rhss[i] is ExprRhs erhs)
                {
                    symbol.Expr = erhs.Expr;
                    symbol.InitStmt = up;
                }
                else
                {
                    symbol.IsUpdated = true;
                }
            }
        }
    }
}
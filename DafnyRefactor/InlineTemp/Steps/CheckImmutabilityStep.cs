using System.Linq;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    public class CheckImmutabilityStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            var visitor = new InlineRetrieveVisitor(state.Program, state.SymbolTable);
            visitor.Execute();
            if (state.InlineSymbol.Expr == null)
            {
                state.Errors.Add(
                    $"Error: variable {state.InlineSymbol.Name} located on {state.InlineOptions.VarLine}:{state.InlineOptions.VarColumn} is never initialized.");
                return;
            }

            if (state.InlineSymbol.IsUpdated)
            {
                state.Errors.Add(
                    $"Error: variable {state.InlineSymbol.Name} located on {state.InlineOptions.VarLine}:{state.InlineOptions.VarColumn} is not constant.");
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
                // TODO: Analyse if commented lines are equivalent with new implementation
                //var erhs = (ExprRhs) up.Rhss[i];
                //symbol.Expr = erhs.Expr;
                var assign = up.Rhss[i];
                symbol.Expr = assign.SubExpressions.FirstOrDefault();
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
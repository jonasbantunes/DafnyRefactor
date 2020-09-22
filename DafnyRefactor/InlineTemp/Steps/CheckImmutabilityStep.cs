using System;
using System.Linq;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    public class CheckImmutabilityStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.InlineSymbol == null || state.Program == null || state.SymbolTable == null)
                throw new ArgumentNullException();

            var visitor = new InlineRetrieveVisitor(state.Program, state.SymbolTable);
            visitor.Execute();
            if (state.InlineSymbol.Expr == null)
            {
                state.AddError(
                    $"Error: variable {state.InlineSymbol.Name} located on {state.InlineOptions.VarLine}:{state.InlineOptions.VarColumn} is never initialized.");
                return;
            }

            if (state.InlineSymbol.IsUpdated)
            {
                state.AddError(
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
            if (program == null || rootTable == null) throw new ArgumentNullException();

            this.rootTable = rootTable;
        }

        protected override void Visit(VarDeclStmt vds)
        {
            if (vds == null) throw new ArgumentNullException();

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
            if (up?.Lhss == null) throw new ArgumentNullException();

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
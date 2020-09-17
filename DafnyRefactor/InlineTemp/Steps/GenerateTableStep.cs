using DafnyRefactor.Utils;
using DafnyRefactor.Utils.SymbolTable;

namespace DafnyRefactor.InlineTemp.Steps
{
    public class GenerateTableStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            // TODO: improve code
            var tableGenerator =
                new SymbolTableGenerator<InlineTable.InlineTable>(state.Program);
            tableGenerator.Execute();
            state.SymbolTable = tableGenerator.GeneratedTable;
            base.Handle(state);
        }
    }
}
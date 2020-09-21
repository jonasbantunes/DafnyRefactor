using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    public class GenerateTableStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            // TODO: improve code
            var tableGenerator =
                new SymbolTableGenerator<InlineTable>(state.Program);
            tableGenerator.Execute();
            state.SymbolTable = tableGenerator.GeneratedTable;
            base.Handle(state);
        }
    }
}
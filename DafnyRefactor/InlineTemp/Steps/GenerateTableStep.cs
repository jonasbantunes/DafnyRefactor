using DafnyRefactor.Utils;
using DafnyRefactor.Utils.SymbolTable;

namespace DafnyRefactor.InlineTemp.Steps
{
    public class GenerateTableStep : RefactorStep<InlineState>
    {
        public override void Handle(InlineState state)
        {
            // TODO: improve code
            var tableGenerator =
                new SymbolTableGenerator<InlineTable.InlineTable>(state.program);
            tableGenerator.Execute();
            state.symbolTable = tableGenerator.GeneratedTable;
            base.Handle(state);
        }
    }
}
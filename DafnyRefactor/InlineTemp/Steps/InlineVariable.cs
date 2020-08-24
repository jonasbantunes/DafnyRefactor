using DafnyRefactor.Utils.SymbolTable;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp.Steps
{
    public class InlineVariable
    {
        public Expression expr = null;
        public bool isUpdated = false;
        public UpdateStmt initStmt = null;
        public string Name => TableDeclaration.Name;
        public SymbolTableDeclaration TableDeclaration { get; protected set; }

        public InlineVariable(SymbolTableDeclaration tableDeclaration)
        {
            TableDeclaration = tableDeclaration;
        }
    }
}
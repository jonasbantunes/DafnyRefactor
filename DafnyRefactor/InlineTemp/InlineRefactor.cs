namespace Microsoft.Dafny
{
    public class InlineRefactor
    {
        public Program program;
        public string method;
        public string name;

        public InlineRefactor(Program program, string method, string name)
        {
            this.program = program;
            this.method = method;
            this.name = name;
        }

        public void refactor()
        {
            var tableGenerator = new SymbolTableGenerator
            {
                program = program
            };
            tableGenerator.execute();
            var symbolTable = tableGenerator.table;

            var locateVariable = new LocateVariableStep()
            {
                program = program,
                table = symbolTable,
                varLine = 2,
                varColumn = 7
                // varLine = 13,
                // varColumn = 9
            };
            locateVariable.execute();
            SymbolTableDeclaration declaration = locateVariable.found;

            var inlineRetriever = new InlineRetrieveStep
            {
                program = program,
                table = symbolTable,
                declaration = declaration
            };
            inlineRetriever.execute();
            var inVar = inlineRetriever.inlineVar;

            if (inVar.isUpdated)
            {
                return;
            }

            var refactor = new InlineRefactorStep
            {
                program = program,
                inlineVar = inVar,
                table = symbolTable
            };

            refactor.execute();
        }
    }
}

using System;
using System.IO;
using System.Linq;

namespace Microsoft.Dafny
{
    public class InlineRefactor
    {
        protected readonly ApplyInlineTempOptions options;
        protected readonly Program program;
        protected readonly int varLine;
        protected readonly int varColumn;
        public int ExitCode { get; protected set; } = 0;

        public InlineRefactor(ApplyInlineTempOptions options, Program program, int varLine, int varColumn)
        {
            this.options = options;
            this.program = program;
            this.varLine = varLine;
            this.varColumn = varColumn;
        }

        public void Refactor()
        {
            var tableGenerator = new SymbolTableGenerator(program);
            tableGenerator.Execute();
            var symbolTable = tableGenerator.GeneratedTable;

            var locateVariable = new LocateVariableStep(program, symbolTable, varLine, varColumn);
            locateVariable.Execute();
            SymbolTableDeclaration declaration = locateVariable.FoundDeclaration;
            if (declaration == null)
            {
                Console.Error.WriteLine($"Error: can't locate variable on line {varLine} and column {varColumn}.");
                ExitCode = 2;
                return;
            }

            var inlineRetriever = new InlineRetrieveStep(program, symbolTable, declaration);
            inlineRetriever.Execute();
            var inVar = inlineRetriever.InlineVar;

            if (inVar.isUpdated)
            {
                Console.Error.WriteLine($"Error: variable {inVar.Name} located on {varLine}:{varColumn} is not constant.");
                ExitCode = 2;
                return;
            }
            var refactor = new InlineRefactorStep(options.FilePath, program, symbolTable, inVar);
            refactor.Execute();
            var replacedEdits = refactor.Edits;

            var remover = new RemoveRefactoredDeclarationStep(program, symbolTable, inVar.tableDeclaration);
            remover.Execute();
            var removedEdits = remover.Edits;

            // TODO: put this on a Step class
            var edits = replacedEdits.Concat(removedEdits).ToList();
            string source = File.ReadAllText(options.FilePath);
            var sourceEditor = new SourceEditor(source, edits);
            sourceEditor.Apply();
            if (options.Stdout)
            {
                Console.WriteLine(sourceEditor.Source);
            }
            else
            {
                File.WriteAllText(options.FilePath, sourceEditor.Source);
            }
        }
    }
}

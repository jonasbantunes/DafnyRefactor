using System;
using System.Collections.Generic;
using System.Linq;
using Bpl = Microsoft.Boogie;

namespace Microsoft.Dafny
{
    public class InlineRefactor
    {
        protected readonly ApplyInlineTempOptions options;
        protected Program program;
        public int ExitCode { get; protected set; } = 0;

        public InlineRefactor(ApplyInlineTempOptions options)
        {
            this.options = options;
        }

        public void Refactor()
        {
            /* STEP 1: INITIALIZE PROGRAM */
            var programLoader = new DafnyProgramLoader(options.FilePath);
            programLoader.Load();
            program = programLoader.Program;
            if (program == null)
            {
                Console.Error.WriteLine($"Error: can't open {options.FilePath}");
                ExitCode = (int)DafnyDriver.ExitValue.DAFNY_ERROR;
                return;
            }

            /* STEP 2: GENERATE SYMBOL TABLE */
            var tableGenerator = new SymbolTableGenerator(program);
            tableGenerator.Execute();
            var symbolTable = tableGenerator.GeneratedTable;

            /* STEP 3: LOCATE INLINE VARIABLE*/
            var locateVariable = new LocateVariableStep(program, symbolTable, options.VarLine, options.VarColumn);
            locateVariable.Execute();
            SymbolTableDeclaration declaration = locateVariable.FoundDeclaration;
            if (declaration == null)
            {
                Console.Error.WriteLine($"Error: can't locate variable on line {options.VarLine} and column {options.VarColumn}.");
                ExitCode = (int)DafnyDriver.ExitValue.DAFNY_ERROR;
                return;
            }

            /* STEP 4: RETRIEVE INLINE VARIABLE INFO */
            var inlineRetriever = new InlineRetrieveStep(program, symbolTable, declaration);
            inlineRetriever.Execute();
            var inVar = inlineRetriever.InlineVar;
            if (inVar.isUpdated)
            {
                Console.Error.WriteLine($"Error: variable {inVar.Name} located on {options.VarLine}:{options.VarColumn} is not constant.");
                ExitCode = (int)DafnyDriver.ExitValue.DAFNY_ERROR;
                return;
            }

            /* STEP 5: APPLY REFACTOR */
            var refactor = new InlineRefactorStep(program, symbolTable, inVar);
            refactor.Execute();
            var replacedEdits = refactor.Edits;

            /* STEP 6: REMOVE VARIABLE DECLARATION */
            var remover = new RemoveRefactoredDeclarationStep(program, symbolTable, inVar.tableDeclaration);
            remover.Execute();
            var removedEdits = remover.Edits;

            /* STEP 7: SAVE CHANGES */
            var edits = replacedEdits.Concat(removedEdits).ToList();
            var saveChanges = new SaveChangesStep(options.FilePath, edits, options.Stdout);
            saveChanges.Save();
        }
    }
}

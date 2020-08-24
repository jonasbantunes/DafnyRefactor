using System;
using System.IO;
using System.Linq;
using DafnyRefactor.InlineTemp.Steps;
using DafnyRefactor.Utils;
using DafnyRefactor.Utils.CommandLineOptions;
using DafnyRefactor.Utils.SymbolTable;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp
{
    public class InlineRefactor
    {
        protected readonly ApplyInlineTempOptions options;
        protected Program program;
        protected TextWriter consoleOutput;
        protected TextWriter consoleError;
        public int ExitCode { get; protected set; }

        public InlineRefactor(ApplyInlineTempOptions options)
        {
            this.options = options;
        }

        public void Refactor()
        {
            consoleOutput = Console.Out;
            consoleError = Console.Error;
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);

            /* STEP 1: INITIALIZE PROGRAM */
            var programLoader = new DafnyProgramLoader(options.FilePath);
            programLoader.Load();
            program = programLoader.Program;
            if (program == null)
            {
                consoleError.WriteLine($"Error: can't open {options.FilePath}");
                ExitCode = (int) DafnyDriver.ExitValue.DAFNY_ERROR;
                return;
            }

            /* STEP 2: GENERATE SYMBOL TABLE */
            var tableGenerator = new SymbolTableGenerator(program);
            tableGenerator.Execute();
            var symbolTable = tableGenerator.GeneratedTable;

            /* STEP 3: LOCATE VARIABLE*/
            var locateVariable = new LocateVariableStep(program, symbolTable, options.VarLine, options.VarColumn);
            locateVariable.Execute();
            SymbolTableDeclaration declaration = locateVariable.FoundDeclaration;
            if (declaration == null)
            {
                consoleError.WriteLine(
                    $"Error: can't locate variable on line {options.VarLine} and column {options.VarColumn}.");
                ExitCode = (int) DafnyDriver.ExitValue.DAFNY_ERROR;
                return;
            }

            /* STEP 4: RETRIEVE VARIABLE INFO */
            var inlineRetriever = new InlineRetrieveStep(program, symbolTable, declaration);
            inlineRetriever.Execute();
            var inlineEdits = inlineRetriever.Edits;
            var inVar = inlineRetriever.InlineVar;
            if (inVar.expr == null)
            {
                consoleError.WriteLine(
                    $"Error: variable {inVar.Name} located on {options.VarLine}:{options.VarColumn} is never initialized.");
                ExitCode = (int) DafnyDriver.ExitValue.DAFNY_ERROR;
                return;
            }
            else if (inVar.isUpdated)
            {
                consoleError.WriteLine(
                    $"Error: variable {inVar.Name} located on {options.VarLine}:{options.VarColumn} is not constant.");
                ExitCode = (int) DafnyDriver.ExitValue.DAFNY_ERROR;
                return;
            }

            /* STEP 5: CHECK IF VARIABLE IS CONSTANT */
            var immutabilitChecker = new InlineImmutabilityCheckStep(options.FilePath, inlineEdits);
            immutabilitChecker.Execute();
            if (!immutabilitChecker.IsConstant)
            {
                consoleError.WriteLine(
                    $"Error: variable {inVar.Name} located on {options.VarLine}:{options.VarColumn} is not constant according with theorem prover.");
                ExitCode = (int) DafnyDriver.ExitValue.DAFNY_ERROR;
                return;
            }

            /* STEP 6: APPLY REFACTOR */
            var refactor = new InlineRefactorStep(program, symbolTable, inVar);
            refactor.Execute();
            var replacedEdits = refactor.Edits;

            /* STEP 7: REMOVE VARIABLE DECLARATION */
            var remover = new RemoveRefactoredDeclarationStep(program, symbolTable, inVar);
            remover.Execute();
            var removedEdits = remover.Edits;

            /* STEP 8: SAVE CHANGES */
            var edits = replacedEdits.Concat(removedEdits).ToList();
            var saveChanges = new SaveChangesStep(edits, options);
            saveChanges.Save();

            if (!saveChanges.ChangesInvalidateSource) return;

            consoleError.WriteLine("Error: refactor invalidate source");
            ExitCode = (int) DafnyDriver.ExitValue.DAFNY_ERROR;
        }
    }
}
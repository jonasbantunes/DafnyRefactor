using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using DafnyRefactor.ExtractVariable;
using DafnyRefactor.InlineTemp;
using DafnyRefactor.MoveMethod;
using DafnyRefactor.MoveMethodToAssociated;
using DafnyRefactor.Utils;
using Microsoft.Dafny;
using Parser = CommandLine.Parser;
using Type = System.Type;

namespace DafnyRefactor
{
    /// <summary>
    ///     The entrypoint of <c>DafnyRefactor</c> project.
    ///     ParseVariables args from CLI and call the adequate <c>Refactor</c> class.
    /// </summary>
    public static class DafnyRefactorDriver
    {
        private static int _exitCode = (int) DafnyDriver.ExitValue.VERIFIED;
        public static TextWriter consoleOutput;
        public static TextWriter consoleError;

        public static int Main(string[] args)
        {
            Type[] types =
            {
                typeof(ApplyInlineTempOptions), typeof(ApplyExtractVariableOptions), typeof(ApplyMoveMethodOptions),
                typeof(ApplyMoveToAssociatedOptions), typeof(GetMoveMethodParamsOptions),
                typeof(GetMoveToAssociatedParamsOptions)
            };
            var parsedArgs = Parser.Default.ParseArguments(args, types);
            return parsedArgs.MapResult(Run, HandleParseError);
        }

        private static int HandleParseError(IEnumerable<Error> errs)
        {
            var errors = errs as Error[] ?? errs.ToArray();
            if (errors.IsVersion())
            {
                Console.WriteLine("Version Request");
                return 0;
            }

            if (errors.IsHelp())
            {
                Console.WriteLine("Help Request");
                return 0;
            }

            Console.WriteLine("Parser Fail");
            return (int) DafnyDriver.ExitValue.DAFNY_ERROR;
        }

        private static int Run(object obj)
        {
            switch (obj)
            {
                case ApplyOptions applyOptions:
                    return Run(applyOptions);
            }

            return (int) DafnyDriver.ExitValue.DAFNY_ERROR;
        }

        private static int Run(ApplyOptions options)
        {
            SetupConsole();

            switch (options)
            {
                case ApplyInlineTempOptions inlineTempOptions:
                    var inlineRefactor = new InlineRefactor(inlineTempOptions);
                    inlineRefactor.Apply();
                    _exitCode = inlineRefactor.ExitCode;
                    break;
                case ApplyExtractVariableOptions extractVariableOptions:
                    var extractVariableRefactor = new ExtractVariableRefactor(extractVariableOptions);
                    extractVariableRefactor.Apply();
                    _exitCode = extractVariableRefactor.ExitCode;
                    break;
                case ApplyMoveMethodOptions moveMethodOptions:
                    var moveMethodRefactor = new MoveMethodRefactor(moveMethodOptions);
                    moveMethodRefactor.Apply();
                    _exitCode = moveMethodRefactor.ExitCode;
                    break;
                case GetMoveMethodParamsOptions getMoveMethodOptions:
                    var moveMethodParamsGetter = new GetMoveMethodParams(getMoveMethodOptions);
                    moveMethodParamsGetter.Apply();
                    _exitCode = moveMethodParamsGetter.ExitCode;
                    break;
                case ApplyMoveToAssociatedOptions moveToAssociatedOptions:
                    _exitCode = MoveToAssociatedRefactor.DoRefactor(moveToAssociatedOptions);
                    break;
                case GetMoveToAssociatedParamsOptions getMoveToAssociatedOptions:
                    _exitCode = GetMoveToAssociatedParams.DoRefactor(getMoveToAssociatedOptions);
                    break;
                default:
                    _exitCode = (int) DafnyDriver.ExitValue.DAFNY_ERROR;
                    break;
            }

            return _exitCode;
        }

        /// <summary>
        ///     Hide stdin and stdout references from third-party libraries.
        ///     Without this, the Dafny compiler will output several log messages.
        /// </summary>
        private static void SetupConsole()
        {
            consoleOutput = Console.Out;
            consoleError = Console.Error;
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.ExtractVariable;
using Microsoft.DafnyRefactor.InlineTemp;
using Microsoft.DafnyRefactor.Utils;
using Parser = CommandLine.Parser;
using Type = System.Type;

namespace DafnyRefactor
{
    /// <summary>
    ///     The entrypoint of <c>DafnyRefactor</c> project.
    ///     Parse args from CLI and call the adequate <c>Refactor</c> class.
    /// </summary>
    public class DafnyRefactorDriver
    {
        protected static int exitCode = (int) DafnyDriver.ExitValue.VERIFIED;
        public static TextWriter consoleOutput;
        public static TextWriter consoleError;

        public static int Main(string[] args)
        {
            Type[] types = {typeof(ApplyInlineTempOptions), typeof(ApplyExtractVariableOptions)};
            var parsedArgs = Parser.Default.ParseArguments(args, types);
            return parsedArgs.MapResult(Run, HandleParseError);
        }

        protected static int HandleParseError(IEnumerable<Error> errs)
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

        protected static int Run(object obj)
        {
            switch (obj)
            {
                case ApplyOptions applyOptions:
                    return Run(applyOptions);
            }

            return (int) DafnyDriver.ExitValue.DAFNY_ERROR;
        }

        protected static int Run(ApplyOptions options)
        {
            SetupConsole();

            switch (options)
            {
                case ApplyInlineTempOptions inlineTempOptions:
                    var inlineRefactor = new InlineRefactor(inlineTempOptions);
                    inlineRefactor.Apply();
                    exitCode = inlineRefactor.ExitCode;
                    break;
                case ApplyExtractVariableOptions extractVariableOptions:
                    var extractVariableRefactor = new ExtractVariableRefactor(extractVariableOptions);
                    extractVariableRefactor.Apply();
                    exitCode = extractVariableRefactor.ExitCode;
                    break;
                default:
                    exitCode = (int) DafnyDriver.ExitValue.DAFNY_ERROR;
                    break;
            }

            return exitCode;
        }

        /// <summary>
        ///     Hide stdin and stdout references from third-party libraries.
        ///     Without this, the Dafny compiler will output several log messages.
        /// </summary>
        protected static void SetupConsole()
        {
            consoleOutput = Console.Out;
            consoleError = Console.Error;
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
        }
    }
}
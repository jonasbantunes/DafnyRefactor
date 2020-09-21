using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.InlineTemp;
using Microsoft.DafnyRefactor.Utils;
using Parser = CommandLine.Parser;
using Type = System.Type;

namespace DafnyRefactor
{
    public class DafnyRefactorDriver
    {
        protected static int exitCode = (int) DafnyDriver.ExitValue.VERIFIED;
        public static TextWriter consoleOutput;
        public static TextWriter consoleError;

        public static int Main(string[] args)
        {
            Type[] types = {typeof(ApplyInlineTempOptions)};
            return Parser.Default.ParseArguments(args, types).MapResult(Run, HandleParseError);
        }

        public static void SetupConsole()
        {
            consoleOutput = Console.Out;
            consoleError = Console.Error;
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
        }

        public static int HandleParseError(IEnumerable<Error> errs)
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

        public static int Run(object obj)
        {
            switch (obj)
            {
                case ApplyOptions applyOptions:
                    return Run(applyOptions);
            }

            return 0;
        }

        public static int Run(ApplyOptions options)
        {
            SetupConsole();

            switch (options)
            {
                case ApplyInlineTempOptions inlineTempOptions:
                    var refactor = new InlineRefactor(inlineTempOptions);
                    refactor.Apply();
                    exitCode = refactor.ExitCode;
                    break;
                default:
                    exitCode = (int) DafnyDriver.ExitValue.DAFNY_ERROR;
                    break;
            }

            return exitCode;
        }
    }
}
using System;
using System.Collections.Generic;
using CommandLine;

namespace Microsoft.Dafny
{
    public class DafnyRefactorDriver
    {
        protected static int exitCode = (int)DafnyDriver.ExitValue.VERIFIED;

        public static int Main(string[] args)
        {
            System.Type[] types = { typeof(ApplyInlineTempOptions) };
            return CommandLine.Parser.Default.ParseArguments(args, types).MapResult(options => Run(options), errs => HandleParseError(errs));
        }

        public static int HandleParseError(IEnumerable<Error> errs)
        {
            if (errs.IsVersion())
            {
                Console.WriteLine("Version Request");
                return 0;
            }

            if (errs.IsHelp())
            {
                Console.WriteLine("Help Request");
                return 0;
            }
            Console.WriteLine("Parser Fail");
            return (int)DafnyDriver.ExitValue.DAFNY_ERROR;
        }

        public static int Run(object obj)
        {
            switch (obj)
            {
                case IApplyOptions applyOptions:
                    return Run(applyOptions);
            }

            return 0;
        }

        public static int Run(IApplyOptions options)
        {
            switch (options)
            {
                case ApplyInlineTempOptions inlineTempOptions:
                    var refactor = new InlineRefactor(inlineTempOptions);
                    refactor.Refactor();
                    exitCode = refactor.ExitCode;
                    break;
                default:
                    exitCode = (int)DafnyDriver.ExitValue.DAFNY_ERROR;
                    break;
            }

            return exitCode;
        }
    }
}

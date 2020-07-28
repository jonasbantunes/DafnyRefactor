using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Bpl = Microsoft.Boogie;

namespace Microsoft.Dafny
{
    public class DafnyRefactorDriver : DafnyDriver
    {
        protected static int exitCode = (int)ExitValue.VERIFIED;
        protected static Program program;

        public static new int Main(string[] args)
        {
            int ret = 0;
            var thread = new System.Threading.Thread(
              new System.Threading.ThreadStart(() =>
              { ret = ThreadMain(args); }),
                0x10000000);
            thread.Start();
            thread.Join();
            return ret;
        }

        public static new int ThreadMain(string[] args)
        {
            InitProgram(args);
            if (exitCode == (int)ExitValue.DAFNY_ERROR)
            {
                return exitCode;
            }
            ApplyRefactor(args);
            return exitCode;
        }

        protected static void InitProgram(string[] args)
        {
            Contract.Requires(cce.NonNullElements(args));

            ErrorReporter reporter = new ConsoleErrorReporter();
            DafnyOptions.Install(new DafnyOptions(reporter));
            Bpl.CommandLineOptions.Clo.ShowEnv = Bpl.CommandLineOptions.ShowEnvironment.Never;
            DafnyOptions.O.PrintMode = DafnyOptions.PrintModes.DllEmbed;

            var dafnyFiles = new List<DafnyFile>
            {
                new DafnyFile(args[0])
            };

            string err = Dafny.Main.Parse(dafnyFiles, "the program", reporter, out program);
            if (err != null)
            {
                exitCode = (int)ExitValue.DAFNY_ERROR;
            }
        }

        protected static void ApplyRefactor(string[] args)
        {
            switch (args[1])
            {
                case "inline-temp":
                    if (args.Length != 4)
                    {
                        exitCode = (int)ExitValue.DAFNY_ERROR;
                        return;
                    }

                    int line = int.Parse(args[2]);
                    int column = int.Parse(args[3]);
                    var refactor = new InlineRefactor(program, line, column);
                    refactor.Refactor();
                    exitCode = refactor.ExitCode;
                    break;
                default:
                    exitCode = (int)ExitValue.DAFNY_ERROR;
                    return;
            }

            if (exitCode == 0)
            {
                Dafny.Main.MaybePrintProgram(program, args[0], false);
                Console.WriteLine("Refactor successfully applied");
            }
        }
    }
}

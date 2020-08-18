using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Bpl = Microsoft.Boogie;
using CommandLine;
using CommandLine.Text;

namespace Microsoft.Dafny
{
    public interface IApplyOptions
    {
        string FilePath { get; set; }
        bool Stdout { get; set; }
    }

    [Verb("apply-inline-temp", HelpText = "Apply a refactor")]
    public class ApplyInlineTempOptions : IApplyOptions
    {
        [Value(0, MetaValue = "filePath", Required = true)]
        public string FilePath { get; set; }

        [Value(1, MetaValue = "varLine", Required = true)]
        public int VarLine { get; set; }

        [Value(2, MetaValue = "varColumn", Required = true)]
        public int VarColumn { get; set; }

        [Option(Default = false, HelpText = "Redirect applied refactor to stdout")]
        public bool Stdout { get; set; }

        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
                    new Example("Apply an inline refator", new ApplyInlineTempOptions{ FilePath = "example.dfy", VarLine= 2, VarColumn =7 })
                };
            }
        }
    }

    public class DafnyRefactorDriver : DafnyDriver
    {
        protected static int exitCode = (int)ExitValue.VERIFIED;
        protected static Program program;

        // TODO: Check if thread is needed
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
            return (int)ExitValue.DAFNY_ERROR;
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
            InitProgram(options.FilePath);
            if (exitCode == (int)ExitValue.DAFNY_ERROR)
            {
                return exitCode;
            }
            ApplyRefactor(options);
            return exitCode;
        }


        protected static void InitProgram(string filePath)
        {
            ErrorReporter reporter = new ConsoleErrorReporter();
            DafnyOptions.Install(new DafnyOptions(reporter));
            Bpl.CommandLineOptions.Clo.ShowEnv = Bpl.CommandLineOptions.ShowEnvironment.Never;
            DafnyOptions.O.PrintMode = DafnyOptions.PrintModes.DllEmbed;

            var dafnyFiles = new List<DafnyFile>
            {
                new DafnyFile(filePath)
            };

            string err = Dafny.Main.Parse(dafnyFiles, "the program", reporter, out program);
            if (err != null)
            {
                exitCode = (int)ExitValue.DAFNY_ERROR;
            }
        }

        protected static void ApplyRefactor(IApplyOptions options)
        {
            switch (options)
            {
                case ApplyInlineTempOptions inlineTempOptions:
                    var refactor = new InlineRefactor(inlineTempOptions, program, inlineTempOptions.VarLine, inlineTempOptions.VarColumn);
                    refactor.Refactor();
                    exitCode = refactor.ExitCode;
                    break;
                default:
                    exitCode = (int)ExitValue.DAFNY_ERROR;
                    return;
            }

            if (exitCode == 0)
            {
                //if (options.Stdout)
                //{
                //    Dafny.Main.MaybePrintProgram(program, "-", false);
                //}
                //else
                //{
                //    Dafny.Main.MaybePrintProgram(program, options.FilePath, false);
                //}
                Console.WriteLine("Refactor successfully applied");
            }
        }
    }
}

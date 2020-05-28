using System.Collections.Generic;
using System.Diagnostics.Contracts;

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
            initProgram(args);
            if (exitCode == (int)ExitValue.DAFNY_ERROR)
            {
                return exitCode;
            }
            applyRefactor();
            return exitCode;
        }

        protected static void initProgram(string[] args)
        {
            Contract.Requires(cce.NonNullElements(args));

            ErrorReporter reporter = new ConsoleErrorReporter();
            DafnyOptions.Install(new DafnyOptions(reporter));

            var dafnyFiles = new List<DafnyFile>();
            dafnyFiles.Add(new DafnyFile(args[0]));

            string err = Dafny.Main.Parse(dafnyFiles, "the program", reporter, out program);
            if (err != null)
            {
                exitCode = (int)ExitValue.DAFNY_ERROR;
            }
        }

        protected static void applyRefactor()
        {
            var refactor = new InlineRefactor(program, "Main", "x");
            refactor.refactor();
            Dafny.Main.MaybePrintProgram(program, "-", false);
        }
    }
}

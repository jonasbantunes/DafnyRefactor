using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Microsoft.Dafny
{
    public partial class DafnyRefactorDriver : DafnyDriver
    {
        public static new int Main(string[] args)
        {
            int ret = 0;
            var thread = new System.Threading.Thread(
              new System.Threading.ThreadStart(() =>
              { ret = ThreadMain(args); }),
                0x10000000); // 256MB stack size to prevent stack overflow
            thread.Start();
            thread.Join();
            return ret;
        }

        public static new int ThreadMain(string[] args)
        {
            Contract.Requires(cce.NonNullElements(args));

            ErrorReporter reporter = new ConsoleErrorReporter();
            DafnyOptions.Install(new DafnyOptions(reporter));

            var dafnyFiles = new List<DafnyFile>();
            dafnyFiles.Add(new DafnyFile(args[0]));

            Program program;
            string err = Dafny.Main.Parse(dafnyFiles, "the program", reporter, out program);
            if (err != null)
            {
                return (int)ExitValue.DAFNY_ERROR;
            }

            var method = "Main";
            var name = "x";
            var inlineRetriever = new InlineRetrieveStep(program, method, name);
            inlineRetriever.execute();
            var inVar = inlineRetriever.inlineVar;
            var refactor = new InlineRefactorStep(program, inVar);
            refactor.execute();

            Dafny.Main.MaybePrintProgram(program, "-", false);

            return (int)ExitValue.VERIFIED;
        }
    }
}

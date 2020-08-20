using System.Collections.Generic;

namespace Microsoft.Dafny
{
    public class DafnyProgramLoader
    {
        protected string filePath;
        public Program Program { get; protected set; }

        public DafnyProgramLoader(string filePath)
        {
            this.filePath = filePath;
        }

        public void Load()
        {
            ErrorReporter reporter = new ConsoleErrorReporter();
            DafnyOptions.Install(new DafnyOptions(reporter));

            Program = null;
            var dafnyFiles = new List<DafnyFile>();
            try
            {
                dafnyFiles.Add(new DafnyFile(filePath));
            }
            catch (IllegalDafnyFile)
            {
                return;
            }


            string err = Main.Parse(dafnyFiles, "the program", reporter, out Program tempProgram);
            if (err == null)
            {
                Program = tempProgram;
            }
        }
    }
}

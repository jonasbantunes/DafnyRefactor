using System.Collections.Generic;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils
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
            if (!IsFileValid()) return;

            var dafnyFiles = new List<DafnyFile>();
            try
            {
                dafnyFiles.Add(new DafnyFile(filePath));
            }
            catch (IllegalDafnyFile)
            {
                return;
            }

            var err = Main.Parse(dafnyFiles, "the program", reporter, out var tempProgram);
            if (err == null)
            {
                Program = tempProgram;
            }
        }

        protected bool IsFileValid()
        {
            var res = DafnyDriver.Main(new[] {filePath, "/compile:0"});
            return res == 0;
        }
    }
}
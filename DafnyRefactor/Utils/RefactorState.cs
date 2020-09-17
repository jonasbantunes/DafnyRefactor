using System.Collections.Generic;
using DafnyRefactor.Utils.CommandLineOptions;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils
{
    public class RefactorState
    {
        public List<string> errors;
        public ApplyOptions options;
        public Program program;
        public string tempFilePath;

        public RefactorState(ApplyOptions options)
        {
            this.options = options;
            errors = new List<string>();
        }

        public string FilePath => options.Stdin ? tempFilePath : options.FilePath;
    }
}
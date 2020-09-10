using System.Collections.Generic;
using DafnyRefactor.Utils.CommandLineOptions;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils
{
    public class RefactorState
    {
        public Program program;
        public List<string> errors;
        public ApplyOptions options;
        public string tempFilePath;

        public string FilePath => options.Stdin ? tempFilePath : options.FilePath;

        public RefactorState(ApplyOptions options)
        {
            this.options = options;
            errors = new List<string>();
        }
    }
}
using System.Collections.Generic;
using DafnyRefactor.Utils.CommandLineOptions;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils
{
    public class RefactorState
    {
        public Program program;
        public List<string> errors;
        public IApplyOptions options;

        public RefactorState(IApplyOptions options)
        {
            this.options = options;
            this.errors = new List<string>();
        }
    }
}
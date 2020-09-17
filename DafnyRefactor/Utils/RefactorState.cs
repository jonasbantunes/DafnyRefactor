using System.Collections.Generic;
using DafnyRefactor.Utils.CommandLineOptions;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils
{
    public interface IRefactorState
    {
        List<string> Errors { get; }
        ApplyOptions Options { get; }
        Program Program { get; set; }
        string TempFilePath { get; set; }
        string FilePath { get; }
    }

    public class RefactorState : IRefactorState
    {
        protected ApplyOptions options;

        public RefactorState(ApplyOptions options)
        {
            this.options = options;
            Errors = new List<string>();
        }

        public List<string> Errors { get; }
        public ApplyOptions Options => options;
        public Program Program { get; set; }
        public string TempFilePath { get; set; }
        public string FilePath => Options.Stdin ? TempFilePath : Options.FilePath;
    }
}
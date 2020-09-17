using System.Collections.Generic;
using DafnyRefactor.InlineTemp.InlineTable;
using DafnyRefactor.Utils;
using DafnyRefactor.Utils.CommandLineOptions;
using DafnyRefactor.Utils.SourceEdit;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp
{
    public interface IInlineState : IRefactorState
    {
        ApplyInlineTempOptions InlineOptions { get; }
        IInlineSymbol InlineSymbol { get; set; }
        List<SourceEdit> SourceEdits { get; }
        IInlineTable SymbolTable { get; set; }
    }

    public class InlineState : IInlineState
    {
        protected ApplyInlineTempOptions options;

        public InlineState(ApplyInlineTempOptions options)
        {
            this.options = options;
            SourceEdits = new List<SourceEdit>();
            Errors = new List<string>();
        }

        public ApplyInlineTempOptions InlineOptions => options;
        public IInlineSymbol InlineSymbol { get; set; }
        public List<SourceEdit> SourceEdits { get; }
        public IInlineTable SymbolTable { get; set; }
        public List<string> Errors { get; }
        public ApplyOptions Options => options;
        public Program Program { get; set; }
        public string TempFilePath { get; set; }
        public string FilePath => Options.Stdin ? TempFilePath : Options.FilePath;
        public List<int> StmtDivisors { get; set; }
    }
}
using System;
using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
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
        protected List<string> errors;
        protected ApplyInlineTempOptions options;

        public InlineState(ApplyInlineTempOptions options)
        {
            this.options = options ?? throw new ArgumentNullException();

            SourceEdits = new List<SourceEdit>();
            errors = new List<string>();
        }

        public ApplyInlineTempOptions InlineOptions => options;
        public IInlineSymbol InlineSymbol { get; set; }
        public List<SourceEdit> SourceEdits { get; }
        public IInlineTable SymbolTable { get; set; }
        public List<string> Errors => errors;
        public ApplyOptions Options => options;
        public Program Program { get; set; }
        public string TempFilePath { get; set; }
        public string FilePath => Options.Stdin ? TempFilePath : Options.FilePath;
        public List<int> StmtDivisors { get; set; }

        public void AddError(string description)
        {
            errors.Add(description);
        }
    }
}
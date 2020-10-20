using System;
using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     Represents the state of a "Inline Temp" refactor.
    /// </summary>
    public interface IInlineState : IRefactorState
    {
        ApplyInlineTempOptions InlineOptions { get; }
        IInlineVariable InlineVariable { get; set; }
        IInlineScope RootScope { get; set; }
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
        public IInlineVariable InlineVariable { get; set; }
        public List<SourceEdit> SourceEdits { get; }
        public IInlineScope RootScope { get; set; }
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
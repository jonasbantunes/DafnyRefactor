using System;
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     Represents the state of a "Inline Temp" refactor.
    /// </summary>
    public interface IInlineState : IRefactorState
    {
        ApplyInlineTempOptions InlineOptions { get; }
        IInlineVariable InlineVariable { get; set; }
        IInlineScope RootScope { get; set; }
        int IvrVariablePos { get; set; }
    }

    public class InlineState : IInlineState
    {
        public InlineState(ApplyInlineTempOptions options)
        {
            InlineOptions = options ?? throw new ArgumentNullException();

            SourceEdits = new List<SourceEdit>();
            Errors = new List<string>();
        }

        public ApplyInlineTempOptions InlineOptions { get; }

        public IInlineVariable InlineVariable { get; set; }
        public List<SourceEdit> SourceEdits { get; }
        public string SourceCode { get; set; }
        public IInlineScope RootScope { get; set; }
        public int IvrVariablePos { get; set; }
        public List<string> Errors { get; }

        public ApplyOptions Options => InlineOptions;
        public Program Program { get; set; }
        public string TempFilePath { get; set; }
        public string FilePath => Options.Stdin ? TempFilePath : Options.FilePath;
        public List<int> StmtDivisors { get; set; }

        public void AddError(string description)
        {
            Errors.Add(description);
        }
    }
}
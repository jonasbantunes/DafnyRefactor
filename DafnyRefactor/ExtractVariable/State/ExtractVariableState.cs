using System;
using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public interface IExtractVariableState : IRefactorState
    {
    }

    public class ExtractVariableState : IExtractVariableState
    {
        protected List<string> errors;
        protected List<SourceEdit> sourceEdits;
        protected ApplyExtractVariableOptions options;

        public ExtractVariableState(ApplyExtractVariableOptions options)
        {
            this.options = options ?? throw new ArgumentNullException();

            sourceEdits = new List<SourceEdit>();
            errors = new List<string>();
        }

        public List<string> Errors => errors;
        public ApplyOptions Options => options;
        public Program Program { get; set; }
        public string TempFilePath { get; set; }
        public string FilePath => options.Stdin ? TempFilePath : options.FilePath;
        public List<int> StmtDivisors { get; set; }

        public void AddError(string description)
        {
            errors.Add(description);
        }
    }
}
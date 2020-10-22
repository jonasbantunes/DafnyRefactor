using System;
using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public interface IExtractVariableState : IRefactorState
    {
        // TODO: Think on a better name
        string RawProgram { get; set; }
        Range Selection { get; set; }
        Range ExprRange { get; set; }
        ApplyExtractVariableOptions ExtractVariableOptions { get; }
        Statement ExtractStmt { get; set; }
    }

    public class ExtractVariableState : IExtractVariableState
    {
        protected List<string> errors;
        protected ApplyExtractVariableOptions options;

        public ExtractVariableState(ApplyExtractVariableOptions options)
        {
            this.options = options ?? throw new ArgumentNullException();

            SourceEdits = new List<SourceEdit>();
            errors = new List<string>();
        }

        public List<string> Errors => errors;
        public ApplyOptions Options => options;
        public Program Program { get; set; }
        public string TempFilePath { get; set; }
        public string FilePath => options.Stdin ? TempFilePath : options.FilePath;
        public List<int> StmtDivisors { get; set; }

        public List<SourceEdit> SourceEdits { get; }

        public void AddError(string description)
        {
            errors.Add(description);
        }

        public string RawProgram { get; set; }
        public Range Selection { get; set; }
        public Range ExprRange { get; set; }
        public ApplyExtractVariableOptions ExtractVariableOptions => options;
        public Statement ExtractStmt { get; set; }
    }
}
using System;
using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    /// <summary>
    ///     Represents the state of a "Extract Variable" refactor.
    /// </summary>
    public interface IExtractVariableState : IRefactorState
    {
        string EvSourceCode { get; set; }
        Range EvUserSelection { get; set; }
        Range EvExprRange { get; set; }
        ApplyExtractVariableOptions EvOptions { get; }
        Statement EvStmt { get; set; }
        IExtractVariableScope EvRootScope { get; set; }
        List<IRefactorVariable> EvExprVariables { get; }
    }

    public class ExtractVariableState : IExtractVariableState
    {
        protected List<string> errors;
        protected ApplyExtractVariableOptions options;

        public ExtractVariableState(ApplyExtractVariableOptions options)
        {
            this.options = options ?? throw new ArgumentNullException();

            SourceEdits = new List<SourceEdit>();
            EvExprVariables = new List<IRefactorVariable>();
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

        public string EvSourceCode { get; set; }
        public Range EvUserSelection { get; set; }
        public Range EvExprRange { get; set; }
        public ApplyExtractVariableOptions EvOptions => options;
        public Statement EvStmt { get; set; }
        public IExtractVariableScope EvRootScope { get; set; }
        public List<IRefactorVariable> EvExprVariables { get; }
    }
}
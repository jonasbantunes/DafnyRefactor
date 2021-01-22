using System;
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.ExtractVariable
{
    /// <summary>
    ///     Represents the state of a "Extract Variable" refactor.
    /// </summary>
    public interface IExtractVariableState : IRefactorState
    {
        Range EvUserSelection { get; set; }
        Range EvExprRange { get; set; }
        ApplyExtractVariableOptions EvOptions { get; }
        Statement EvStmt { get; set; }
        IExtractVariableScope EvRootScope { get; set; }
        List<IRefactorVariable> EvExprVariables { get; }
    }

    public class ExtractVariableState : IExtractVariableState
    {
        public ExtractVariableState(ApplyExtractVariableOptions options)
        {
            EvOptions = options ?? throw new ArgumentNullException();

            SourceEdits = new List<SourceEdit>();
            EvExprVariables = new List<IRefactorVariable>();
            Errors = new List<string>();
        }

        public List<string> Errors { get; }

        public ApplyOptions Options => EvOptions;
        public Program Program { get; set; }
        public string TempFilePath { get; set; }
        public string FilePath => EvOptions.Stdin ? TempFilePath : EvOptions.FilePath;
        public List<int> StmtDivisors { get; set; }

        public List<SourceEdit> SourceEdits { get; }
        public string SourceCode { get; set; }

        public void AddError(string description)
        {
            Errors.Add(description);
        }

        public Range EvUserSelection { get; set; }
        public Range EvExprRange { get; set; }
        public ApplyExtractVariableOptions EvOptions { get; }
        public Statement EvStmt { get; set; }
        public IExtractVariableScope EvRootScope { get; set; }
        public List<IRefactorVariable> EvExprVariables { get; }
    }
}
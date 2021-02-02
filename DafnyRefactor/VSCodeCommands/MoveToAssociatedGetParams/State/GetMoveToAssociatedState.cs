using System;
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethodToAssociated
{
    public interface IGetMoveToAssociatedState : IRefactorState
    {
        GetMoveToAssociatedParamsOptions GmtaOptions { get; }
        int GmtaMethodPos { get; set; }
    }

    public class GetMoveToAssociatedState : IGetMoveToAssociatedState
    {
        public GetMoveToAssociatedState(GetMoveToAssociatedParamsOptions gmtaOptions)
        {
            GmtaOptions = gmtaOptions ?? throw new ArgumentNullException();
        }


        public List<string> Errors { get; } = new List<string>();
        public ApplyOptions Options => GmtaOptions;
        public Program Program { get; set; }
        public string TempFilePath { get; set; }
        public string FilePath => GmtaOptions.Stdin ? TempFilePath : GmtaOptions.FilePath;
        public List<int> StmtDivisors { get; set; }
        public List<SourceEdit> SourceEdits { get; } = new List<SourceEdit>();
        public string SourceCode { get; set; }

        public void AddError(string description)
        {
            Errors.Add(description);
        }

        public GetMoveToAssociatedParamsOptions GmtaOptions { get; }
        public int GmtaMethodPos { get; set; }
    }
}
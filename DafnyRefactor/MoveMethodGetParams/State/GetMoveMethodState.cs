using System;
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethod
{
    public interface IGetMoveMethodState : IRefactorState
    {
        GetMoveMethodParamsOptions GmpOptions { get; }
        int GmpMethodPos { get; set; }
    }

    public class GetMoveMethodState : IGetMoveMethodState
    {
        public GetMoveMethodState(GetMoveMethodParamsOptions gmpOptions)
        {
            GmpOptions = gmpOptions ?? throw new ArgumentNullException();
        }

        public List<string> Errors { get; } = new List<string>();
        public ApplyOptions Options => GmpOptions;
        public Program Program { get; set; }
        public string TempFilePath { get; set; }
        public string FilePath => GmpOptions.Stdin ? TempFilePath : GmpOptions.FilePath;
        public List<int> StmtDivisors { get; set; }
        public List<SourceEdit> SourceEdits { get; } = new List<SourceEdit>();
        public string SourceCode { get; set; }

        public void AddError(string description)
        {
            Errors.Add(description);
        }

        public GetMoveMethodParamsOptions GmpOptions { get; }
        public int GmpMethodPos { get; set; }
    }
}
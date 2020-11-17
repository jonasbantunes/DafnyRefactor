using System;
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethod
{
    public interface IMoveMethodState : IRefactorState
    {
        ApplyMoveMethodOptions MvtOptions { get; }
        int MvtUserTarget { get; set; }
        IMvtParam MvtParam { get; set; }
    }

    public class MoveMethodState : IMoveMethodState
    {
        protected ApplyMoveMethodOptions options;

        public MoveMethodState(ApplyMoveMethodOptions options)
        {
            this.options = options ?? throw new ArgumentNullException();

            Errors = new List<string>();
            SourceEdits = new List<SourceEdit>();
        }

        public List<string> Errors { get; }
        public ApplyOptions Options => options;
        public Program Program { get; set; }
        public string TempFilePath { get; set; }
        public string FilePath => options.Stdin ? TempFilePath : options.FilePath;
        public List<int> StmtDivisors { get; set; }
        public List<SourceEdit> SourceEdits { get; }
        public string SourceCode { get; set; }

        public void AddError(string description)
        {
            Errors.Add(description);
        }

        public ApplyMoveMethodOptions MvtOptions => options;
        public int MvtUserTarget { get; set; }
        public IMvtParam MvtParam { get; set; }
    }
}
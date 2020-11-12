using System;
using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.MoveMethod
{
    public interface IMoveMethodState : IRefactorState
    {
        string MvtSourceCode { get; set; }
        ApplyMoveMethodOptions MvtOptions { get; }
        int MvtUserTarget { get; set; }
        Method MvtMethod { get; set; }
        Formal MvtParam { get; set; }
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

        public void AddError(string description)
        {
            Errors.Add(description);
        }

        public string MvtSourceCode { get; set; }
        public ApplyMoveMethodOptions MvtOptions => options;
        public int MvtUserTarget { get; set; }
        public Method MvtMethod { get; set; }
        public Formal MvtParam { get; set; }
    }
}
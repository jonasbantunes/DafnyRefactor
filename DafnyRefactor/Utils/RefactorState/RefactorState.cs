using System.Collections.Generic;
using Microsoft.Dafny;

namespace Microsoft.DafnyRefactor.Utils
{
    public interface IRefactorState
    {
        List<string> Errors { get; }
        ApplyOptions Options { get; }
        Program Program { get; set; }
        string TempFilePath { get; set; }
        string FilePath { get; }
        List<int> StmtDivisors { get; set; }

        void AddError(string description);
    }
}
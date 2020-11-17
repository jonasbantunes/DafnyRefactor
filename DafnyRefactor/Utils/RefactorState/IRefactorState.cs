using System.Collections.Generic;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils
{
    /// <summary>
    ///     Represents the state of a refactor.
    ///     <para>
    ///         This object is normally passed between multiple <c>RefatorStep</c>.
    ///     </para>
    /// </summary>
    public interface IRefactorState
    {
        List<string> Errors { get; }
        ApplyOptions Options { get; }
        Program Program { get; set; }
        string TempFilePath { get; set; }
        string FilePath { get; }
        List<int> StmtDivisors { get; set; }
        List<SourceEdit> SourceEdits { get; }
        string SourceCode { get; set; }

        void AddError(string description);
    }
}
using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.MoveMethodToAssociated
{
    public interface IMoveToAssociatedState : IRefactorState
    {
        ApplyMoveToAssociatedOptions MtaOptions { get; }
        int MtaOriginPos { get; set; }
        Method MtaOriginMethod { get; set; }
        int MtaTargetPos { get; set; }
        Field MtaTargetField { get; set; }
    }

    public class MoveToAssociatedState : IMoveToAssociatedState
    {
        public MoveToAssociatedState(ApplyMoveToAssociatedOptions options)
        {
            MtaOptions = options;
        }

        public List<string> Errors { get; } = new List<string>();
        public ApplyOptions Options => MtaOptions;
        public Program Program { get; set; }
        public string TempFilePath { get; set; }
        public string FilePath => MtaOptions.Stdin ? TempFilePath : MtaOptions.FilePath;
        public List<int> StmtDivisors { get; set; }
        public List<SourceEdit> SourceEdits { get; } = new List<SourceEdit>();
        public string SourceCode { get; set; }

        public void AddError(string description)
        {
            Errors.Add(description);
        }

        public ApplyMoveToAssociatedOptions MtaOptions { get; }
        public int MtaOriginPos { get; set; }
        public Method MtaOriginMethod { get; set; }
        public int MtaTargetPos { get; set; }
        public Field MtaTargetField { get; set; }
    }
}
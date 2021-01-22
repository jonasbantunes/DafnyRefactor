using CommandLine;
using DafnyRefactor.Utils;

namespace DafnyRefactor.MoveMethodToAssociated
{
    [Verb("apply-move-method-to-associated")]
    public class ApplyMoveToAssociatedOptions : ApplyOptions
    {
        [Option('s', "methodPos", Required = true)]
        public string MethodPosition { get; set; }

        [Option('t', "fieldPos", Required = true)]
        public string FieldPosition { get; set; }
    }
}
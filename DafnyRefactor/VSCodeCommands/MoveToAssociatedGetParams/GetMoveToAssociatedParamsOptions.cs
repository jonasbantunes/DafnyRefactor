using CommandLine;
using DafnyRefactor.Utils;

namespace DafnyRefactor.MoveMethodToAssociated
{
    [Verb("get-move-to-associated-params", Hidden = true)]
    public class GetMoveToAssociatedParamsOptions : ApplyOptions
    {
        [Option('p', "methodPos", Required = true)]
        public string MethodPosition { get; set; }
    }
}
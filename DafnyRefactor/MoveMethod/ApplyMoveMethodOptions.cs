using CommandLine;
using DafnyRefactor.Utils;

namespace DafnyRefactor.MoveMethod
{
    [Verb("apply-move-method")]
    public class ApplyMoveMethodOptions : ApplyOptions
    {
        [Option('i', "instancePosition", Required = true)]
        public string InstancePosition { get; set; }
    }
}
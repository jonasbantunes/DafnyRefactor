using CommandLine;
using DafnyRefactor.Utils;

namespace DafnyRefactor.MoveMethod
{
    [Verb("get-move-method-params", Hidden = true)]
    public class GetMoveMethodParamsOptions : ApplyOptions
    {
        [Option('p', "methodPosition", Required = true)]
        public string MethodPosition { get; set; }
    }
}
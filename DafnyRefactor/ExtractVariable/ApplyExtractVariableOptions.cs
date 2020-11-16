using CommandLine;

namespace DafnyRefactor.Utils
{
    [Verb("apply-extract-variable")]
    public class ApplyExtractVariableOptions : ApplyOptions
    {
        [Option('s', "startPosition", Required = true)]
        public string StartPosition { get; set; }

        [Option('e', "endPosition", Required = true)]
        public string EndPosition { get; set; }

        [Option('v', "varName", Required = true)]
        public string VarName { get; set; }
    }
}
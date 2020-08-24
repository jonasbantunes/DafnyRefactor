using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace DafnyRefactor.Utils.CommandLineOptions
{
    [Verb("apply-inline-temp", HelpText = "Apply a refactor")]
    public class ApplyInlineTempOptions : IApplyOptions
    {
        [Value(0, MetaValue = "filePath", Required = true)]
        public string FilePath { get; set; }

        [Value(1, MetaValue = "varLine", Required = true)]
        public int VarLine { get; set; }

        [Value(2, MetaValue = "varColumn", Required = true)]
        public int VarColumn { get; set; }

        [Option(Default = false, HelpText = "Redirect applied refactor to stdout")]
        public bool Stdout { get; set; }

        [Option('o', "output", Default = null, HelpText = "Redirect applied refactor to file")]
        public string Output { get; set; }

        [Usage]
        public static IEnumerable<Example> Examples =>
            new List<Example>()
            {
                new Example("Apply an inline refator",
                    new ApplyInlineTempOptions {FilePath = "example.dfy", VarLine = 2, VarColumn = 7})
            };
    }
}
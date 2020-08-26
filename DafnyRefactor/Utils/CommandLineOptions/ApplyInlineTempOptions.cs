using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace DafnyRefactor.Utils.CommandLineOptions
{
    [Verb("apply-inline-temp", HelpText = "Apply a refactor")]
    public class ApplyInlineTempOptions : IApplyOptions
    {
        [Option('f', "filePath", Group = "input", Default = null)]
        public string FilePath { get; set; }

        [Option(Group = "input", Default = null)]
        public bool Stdin { get; set; }

        [Option('l', "line", Required = true)] public int VarLine { get; set; }

        [Option('c', "column", Required = true)]
        public int VarColumn { get; set; }

        [Option(Group = "output", Default = false, HelpText = "Redirect applied refactor to stdout")]
        public bool Stdout { get; set; }

        [Option('o', "output", Group = "output", Default = null, HelpText = "Redirect applied refactor to file")]
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
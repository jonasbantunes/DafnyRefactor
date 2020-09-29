using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace Microsoft.DafnyRefactor.Utils
{
    /// <summary>
    ///     Represents a set of CLI options to apply an "Inline Temp" refactor.
    /// </summary>
    [Verb("apply-inline-temp", HelpText = "Apply a refactor")]
    public class ApplyInlineTempOptions : ApplyOptions
    {
        [Option('l', "line", Required = true)] public int VarLine { get; set; }

        [Option('c', "column", Required = true)]
        public int VarColumn { get; set; }

        [Usage]
        public static IEnumerable<Example> Examples =>
            new List<Example>
            {
                new Example("Apply an inline refator",
                    new ApplyInlineTempOptions {FilePath = "example.dfy", VarLine = 2, VarColumn = 7})
            };
    }
}
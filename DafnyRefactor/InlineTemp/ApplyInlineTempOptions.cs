using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace DafnyRefactor.Utils
{
    /// <summary>
    ///     Represents a set of CLI options to apply an "Inline Temp" refactor.
    /// </summary>
    [Verb("apply-inline-temp")]
    public class ApplyInlineTempOptions : ApplyOptions
    {
        [Option('p', "position", Required = true)]
        public string Position { get; set; }

        [Usage]
        public static IEnumerable<Example> Examples =>
            new List<Example>
            {
                new Example("Apply an inline refator",
                    new ApplyInlineTempOptions {FilePath = "example.dfy", Position = "2:7"})
            };
    }
}
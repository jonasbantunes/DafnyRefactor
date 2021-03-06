﻿using CommandLine;

namespace DafnyRefactor.Utils
{
    /// <summary>
    ///     Represents a set of base CLI options that should be used on all refactors.
    /// </summary>
    public abstract class ApplyOptions
    {
        [Option('f', "filePath", Group = "input", Default = null)]
        public string FilePath { get; set; }

        [Option(Group = "input", Default = null)]
        public bool Stdin { get; set; }

        [Option(Group = "output", Default = false, HelpText = "Redirect applied refactor to stdout")]
        public bool Stdout { get; set; }

        [Option('o', "output", Group = "output", Default = null, HelpText = "Redirect applied refactor to file")]
        public string Output { get; set; }
    }
}
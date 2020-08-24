using System;
using System.Collections.Generic;
using System.IO;
using DafnyRefactor.Utils.CommandLineOptions;
using DafnyRefactor.Utils.SourceEdit;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp.Steps
{
    public class SaveChangesStep
    {
        protected readonly ApplyInlineTempOptions options;
        protected readonly List<SourceEdit> edits;
        public bool ChangesInvalidateSource { get; protected set; }

        public SaveChangesStep(List<SourceEdit> edits, ApplyInlineTempOptions options)
        {
            this.edits = edits;
            this.options = options;
        }

        public void Save()
        {
            string source = File.ReadAllText(options.FilePath);
            var sourceEditor = new SourceEditor(source, edits);
            sourceEditor.Apply();

            // Check if source is still valid after changes
            string tempPath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".dfy";
            File.WriteAllText(tempPath, sourceEditor.Source);
            var res = DafnyDriver.Main(new[] {tempPath, "/compile:0"});
            File.Delete(tempPath);
            ChangesInvalidateSource = res != 0;

            if (!ChangesInvalidateSource)
            {
                if (options.Stdout)
                {
                    Console.WriteLine(sourceEditor.Source);
                }
                else if (options.Output != null)
                {
                    File.WriteAllText(options.Output, sourceEditor.Source);
                }
                else
                {
                    File.WriteAllText(options.FilePath, sourceEditor.Source);
                }
            }
        }
    }
}
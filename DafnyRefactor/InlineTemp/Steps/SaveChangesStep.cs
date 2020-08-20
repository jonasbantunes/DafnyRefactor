using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Dafny
{
    public class SaveChangesStep
    {
        protected readonly bool stdout;
        protected readonly List<SourceEdit> edits;
        protected readonly string filePath;
        public bool ChangesInvalidateSource { get; protected set; } = false;

        public SaveChangesStep(string filePath, List<SourceEdit> edits, bool stdout)
        {
            this.filePath = filePath;
            this.edits = edits;
            this.stdout = stdout;
        }

        public void Save()
        {
            string source = File.ReadAllText(filePath);
            var sourceEditor = new SourceEditor(source, edits);
            sourceEditor.Apply();

            // Check if source is still valid after changes
            string tempPath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".dfy";
            File.WriteAllText(tempPath, sourceEditor.Source);
            var res = DafnyDriver.Main(new string[] { tempPath, "/compile:0" });
            File.Delete(tempPath);
            ChangesInvalidateSource = res != 0;

            if (!ChangesInvalidateSource)
            {
                if (stdout)
                {
                    Console.WriteLine(sourceEditor.Source);
                }
                else
                {
                    File.WriteAllText(filePath, sourceEditor.Source);
                }
            }
        }
    }
}

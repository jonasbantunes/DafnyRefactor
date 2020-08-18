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

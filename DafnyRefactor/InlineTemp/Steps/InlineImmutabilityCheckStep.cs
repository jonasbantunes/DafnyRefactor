using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Dafny
{
    public class InlineImmutabilityCheckStep
    {
        protected string filePath;
        protected List<SourceEdit> edits;
        public bool IsConstant { get; protected set; }

        public InlineImmutabilityCheckStep(string filePath, List<SourceEdit> edits)
        {
            this.filePath = filePath;
            this.edits = edits;

        }

        public void Execute()
        {
            string source = File.ReadAllText(filePath);
            var sourceEditor = new SourceEditor(source, edits);
            sourceEditor.Apply();

            string tempPath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".dfy";
            File.WriteAllText(tempPath, sourceEditor.Source);
            Console.WriteLine(tempPath);

            var res = DafnyDriver.Main(new string[] { tempPath, "/compile:0" });
            IsConstant = res == 0;
            File.Delete(tempPath);
        }
    }
}

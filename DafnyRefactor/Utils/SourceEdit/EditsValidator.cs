using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Dafny;

namespace Microsoft.DafnyRefactor.Utils
{
    public class EditsValidator
    {
        protected List<SourceEdit> edits;
        protected string filePath;

        public EditsValidator(string filePath, List<SourceEdit> edits)
        {
            if (filePath == null || edits == null) throw new ArgumentNullException();

            this.filePath = filePath;
            this.edits = edits;
        }

        public bool IsValid { get; protected set; }

        public void Execute()
        {
            var source = File.ReadAllText(filePath);
            var sourceEditor = new SourceEditor(source, edits);
            sourceEditor.Apply();

            var tempPath = Path.GetTempPath() + Guid.NewGuid() + ".dfy";
            File.WriteAllText(tempPath, sourceEditor.Source);

            var args = new[] {tempPath, "/compile:0"};
            var res = DafnyDriver.Main(args);
            IsValid = res == 0;
            File.Delete(tempPath);
        }
    }
}
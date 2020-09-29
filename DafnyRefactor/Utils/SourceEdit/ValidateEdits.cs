using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    internal class ValidateEdits
    {
        protected List<SourceEdit> edits;
        protected string filePath;

        public ValidateEdits(string filePath, List<SourceEdit> edits)
        {
            if (filePath == null || edits == null) throw new ArgumentNullException();

            this.filePath = filePath;
            this.edits = edits;
        }

        public bool IsConstant { get; protected set; }

        public void Execute()
        {
            var source = File.ReadAllText(filePath);
            var sourceEditor = new SourceEditor(source, edits);
            sourceEditor.Apply();

            var tempPath = Path.GetTempPath() + Guid.NewGuid() + ".dfy";
            File.WriteAllText(tempPath, sourceEditor.Source);

            var args = new[] {tempPath, "/compile:0"};
            var res = DafnyDriver.Main(args);
            IsConstant = res == 0;
            File.Delete(tempPath);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils
{
    public class EditsValidator
    {
        protected List<SourceEdit> edits;
        protected string filePath;
        protected bool isValid;

        protected EditsValidator(string filePath, List<SourceEdit> edits)
        {
            if (filePath == null || edits == null) throw new ArgumentNullException();

            this.filePath = filePath;
            this.edits = edits;
        }

        protected void Execute()
        {
            var source = File.ReadAllText(filePath);
            var sourceEditor = new SourceEditor(source, edits);
            sourceEditor.Apply();

            var tempPath = Path.GetTempPath() + Guid.NewGuid() + ".dfy";
            File.WriteAllText(tempPath, sourceEditor.Source);

            var args = new[] {tempPath, "/compile:0"};
            var res = DafnyDriver.Main(args);
            isValid = res == 0;
            File.Delete(tempPath);
        }

        public static bool IsValid(List<SourceEdit> edits, string filePath)
        {
            var validator = new EditsValidator(filePath, edits);
            validator.Execute();
            return validator.isValid;
        }
    }
}
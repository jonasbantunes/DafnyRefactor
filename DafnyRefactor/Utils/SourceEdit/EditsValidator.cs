using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils
{
    public class EditsValidator
    {
        private readonly List<SourceEdit> _edits;
        private readonly string _filePath;
        private bool _isValid;

        private EditsValidator(string filePath, List<SourceEdit> edits)
        {
            if (filePath == null || edits == null) throw new ArgumentNullException();

            _filePath = filePath;
            _edits = edits;
        }

        private void Execute()
        {
            var source = File.ReadAllText(_filePath);
            var editedSource = SourceEditor.Edit(source, _edits);

            var tempPath = Path.GetTempPath() + Guid.NewGuid() + ".dfy";
            File.WriteAllText(tempPath, editedSource);

            var args = new[] {tempPath, "/compile:0"};
            var res = DafnyDriver.Main(args);
            _isValid = res == 0;
            File.Delete(tempPath);
        }

        public static bool IsValid(List<SourceEdit> edits, string filePath)
        {
            var validator = new EditsValidator(filePath, edits);
            validator.Execute();
            return validator._isValid;
        }
    }
}
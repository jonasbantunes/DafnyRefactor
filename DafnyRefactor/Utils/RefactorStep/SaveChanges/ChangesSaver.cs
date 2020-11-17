using System;
using System.Collections.Generic;
using System.IO;

namespace DafnyRefactor.Utils
{
    public class ChangesSaver
    {
        private readonly List<SourceEdit> _edits;
        private readonly string _filePath;
        private readonly ApplyOptions _options;

        private bool _changesInvalidateSource;
        private string _sourceCode;

        private ChangesSaver(string filePath, List<SourceEdit> edits, ApplyOptions options)
        {
            if (edits == null || filePath == null || options == null) throw new ArgumentNullException();

            _filePath = filePath;
            _edits = edits;
            _options = options;
        }

        private void Execute()
        {
            ApplyChanges();
            _changesInvalidateSource = !EditsValidator.IsValid(_edits, _filePath);
            if (_changesInvalidateSource) return;

            SaveTo();
        }

        private void ApplyChanges()
        {
            var source = File.ReadAllText(_filePath);
            _sourceCode = SourceEditor.Edit(source, _edits);
        }

        private void SaveTo()
        {
            if (_options.Stdout)
            {
                DafnyRefactorDriver.consoleOutput.Write(_sourceCode);
            }
            else if (_options.Output != null)
            {
                File.WriteAllText(_options.Output, _sourceCode);
            }
            else
            {
                File.WriteAllText(_filePath, _sourceCode);
            }
        }

        public static bool Save(string filePath, List<SourceEdit> edits, ApplyOptions options)
        {
            var saver = new ChangesSaver(filePath, edits, options);
            saver.Execute();
            return saver._changesInvalidateSource;
        }
    }
}
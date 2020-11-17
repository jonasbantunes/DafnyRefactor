using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DafnyRefactor.Utils
{
    /// <summary>
    ///     Apply changes to file according with a list of <c>SourceEdit</c>.
    /// </summary>
    public class SourceEditor
    {
        private readonly List<SourceEdit> _edits;

        private string _sourceCode;

        private SourceEditor(string sourceCode, List<SourceEdit> edits)
        {
            _sourceCode = sourceCode;
            _edits = edits ?? throw new ArgumentNullException();
        }

        private void Apply()
        {
            var sourceBuilder = new StringBuilder(_sourceCode);
            var sortedEdits = _edits.OrderBy(edit => edit.startPos).ToList();
            for (var i = sortedEdits.Count - 1; i >= 0; i--)
            {
                var edit = sortedEdits[i];
                sourceBuilder.Remove(edit.startPos, edit.endPos - edit.startPos);
                sourceBuilder.Insert(edit.startPos, edit.content);
            }

            _sourceCode = sourceBuilder.ToString();
        }

        public static string Edit(string source, List<SourceEdit> edits)
        {
            var editor = new SourceEditor(source, edits);
            editor.Apply();
            return editor._sourceCode;
        }
    }
}
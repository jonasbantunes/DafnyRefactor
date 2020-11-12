using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.DafnyRefactor.Utils
{
    /// <summary>
    ///     Apply changes to file according with a list of <c>SourceEdit</c>.
    /// </summary>
    public class SourceEditor
    {
        protected List<SourceEdit> edits;

        public SourceEditor(string source, List<SourceEdit> edits)
        {
            Source = source;
            this.edits = edits ?? throw new ArgumentNullException();
        }

        public string Source { get; protected set; }

        public void Apply()
        {
            var sourceBuilder = new StringBuilder(Source);
            var sortedEdits = edits.OrderBy(edit => edit.startPos).ToList();
            for (var i = sortedEdits.Count - 1; i >= 0; i--)
            {
                var edit = sortedEdits[i];
                sourceBuilder.Remove(edit.startPos, edit.endPos - edit.startPos);
                sourceBuilder.Insert(edit.startPos, edit.content);
            }

            Source = sourceBuilder.ToString();
        }

        public static string Edit(string source, List<SourceEdit> edits)
        {
            var editor = new SourceEditor(source, edits);
            editor.Apply();
            return editor.Source;
        }
    }
}
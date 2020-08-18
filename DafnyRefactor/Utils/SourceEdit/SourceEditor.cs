using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Dafny
{
    public class SourceEditor
    {
        protected List<SourceEdit> edits;
        public string Source { get; protected set; }

        public SourceEditor(string source, List<SourceEdit> edits)
        {
            Source = source;
            this.edits = edits;
        }

        public void Apply()
        {
            var sourceBuilder = new StringBuilder(Source);
            edits = edits.OrderByDescending(edit => edit.startPos).ToList();
            foreach (SourceEdit edit in edits)
            {
                sourceBuilder.Remove(edit.startPos, edit.endPos - edit.startPos);
                sourceBuilder.Insert(edit.startPos, edit.content);
            }
            Source = sourceBuilder.ToString();
        }
    }
}

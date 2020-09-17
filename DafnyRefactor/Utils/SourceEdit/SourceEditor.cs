using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DafnyRefactor.Utils.SourceEdit
{
    public class SourceEditor
    {
        protected List<SourceEdit> edits;

        public SourceEditor(string source, List<SourceEdit> edits)
        {
            Source = source;
            this.edits = edits;
        }

        public string Source { get; protected set; }

        public void Apply()
        {
            var sourceBuilder = new StringBuilder(Source);
            edits = edits.OrderByDescending(edit => edit.startPos).ToList();
            foreach (var edit in edits)
            {
                sourceBuilder.Remove(edit.startPos, edit.endPos - edit.startPos);
                sourceBuilder.Insert(edit.startPos, edit.content);
            }

            Source = sourceBuilder.ToString();
        }
    }
}
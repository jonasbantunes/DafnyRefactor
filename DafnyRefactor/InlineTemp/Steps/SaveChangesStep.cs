using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DafnyRefactor.Utils;
using DafnyRefactor.Utils.CommandLineOptions;
using DafnyRefactor.Utils.SourceEdit;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp.Steps
{
    public class SaveChangesStep : RefactorStep<InlineState>
    {
        public override void Handle(InlineState state)
        {
            var edits = state.replaceSourceEdits.Concat(state.removeSourceEdits).ToList();
            var changer = new SaveChanges(edits, state.inlineOptions);
            changer.Save();

            if (changer.ChangesInvalidateSource)
            {
                state.errors.Add("Error: refactor invalidate source");
                return;
            }

            base.Handle(state);
        }
    }

    internal class SaveChanges
    {
        protected readonly ApplyInlineTempOptions options;
        protected readonly List<SourceEdit> edits;
        protected SourceEditor sourceEditor;
        public bool ChangesInvalidateSource { get; protected set; }

        public SaveChanges(List<SourceEdit> edits, ApplyInlineTempOptions options)
        {
            this.edits = edits;
            this.options = options;
        }

        public void Save()
        {
            ApplyChanges();
            VerifySourceProof();
            if (ChangesInvalidateSource) return;

            SaveTo();
        }

        protected void ApplyChanges()
        {
            var source = File.ReadAllText(options.FilePath);
            sourceEditor = new SourceEditor(source, edits);
            sourceEditor.Apply();
        }

        protected void VerifySourceProof()
        {
            var tempPath = Path.GetTempPath() + Guid.NewGuid() + ".dfy";
            File.WriteAllText(tempPath, sourceEditor.Source);
            var res = DafnyDriver.Main(new[] {tempPath, "/compile:0"});
            File.Delete(tempPath);

            ChangesInvalidateSource = res != 0;
        }

        protected void SaveTo()
        {
            if (options.Stdout)
            {
                DafnyRefactorDriver.consoleOutput.Write(sourceEditor.Source);
            }
            else if (options.Output != null)
            {
                File.WriteAllText(options.Output, sourceEditor.Source);
            }
            else
            {
                File.WriteAllText(options.FilePath, sourceEditor.Source);
            }
        }
    }
}
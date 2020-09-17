using System;
using System.IO;
using DafnyRefactor.Utils;
using DafnyRefactor.Utils.SourceEdit;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp.Steps
{
    public class SaveChangesStep : RefactorStep<InlineState>
    {
        public override void Handle(InlineState state)
        {
            var changer = new SaveChanges(state);
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
        protected readonly InlineState state;
        protected SourceEditor sourceEditor;

        public SaveChanges(InlineState state)
        {
            this.state = state;
        }

        public bool ChangesInvalidateSource { get; protected set; }

        public void Save()
        {
            ApplyChanges();
            VerifySourceProof();
            if (ChangesInvalidateSource) return;

            SaveTo();
        }

        protected void ApplyChanges()
        {
            var source = File.ReadAllText(state.FilePath);
            sourceEditor = new SourceEditor(source, state.sourceEdits);
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
            if (state.options.Stdout)
            {
                DafnyRefactorDriver.consoleOutput.Write(sourceEditor.Source);
            }
            else if (state.options.Output != null)
            {
                File.WriteAllText(state.options.Output, sourceEditor.Source);
            }
            else
            {
                File.WriteAllText(state.FilePath, sourceEditor.Source);
            }
        }
    }
}
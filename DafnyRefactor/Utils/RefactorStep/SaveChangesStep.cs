using System;
using System.Collections.Generic;
using System.IO;
using DafnyRefactor;
using Microsoft.Dafny;

namespace Microsoft.DafnyRefactor.Utils
{
    /// <summary>
    ///     A <c>RefactorStep</c> that saves a list of <c>SourceEdit</c> to a file.
    /// </summary>
    public class SaveChangesStep<TState> : RefactorStep<TState> where TState : IRefactorState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Errors == null || state.FilePath == null || state.Options == null ||
                state.SourceEdits == null) throw new ArgumentNullException();

            var changer = new ChangesSaver(state.FilePath, state.SourceEdits, state.Options);
            changer.Save();

            if (changer.ChangesInvalidateSource)
            {
                state.AddError("Error: refactor invalidate source");
                return;
            }

            base.Handle(state);
        }
    }

    internal class ChangesSaver
    {
        protected List<SourceEdit> edits;
        protected string filePath;
        protected ApplyOptions options;
        protected SourceEditor sourceEditor;

        public ChangesSaver(string filePath, List<SourceEdit> edits, ApplyOptions options)
        {
            if (edits == null || filePath == null || options == null) throw new ArgumentNullException();

            this.filePath = filePath;
            this.edits = edits;
            this.options = options;
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
            var source = File.ReadAllText(filePath);
            sourceEditor = new SourceEditor(source, edits);
            sourceEditor.Apply();
        }

        // TODO: Use EditsValidator instead
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
                File.WriteAllText(filePath, sourceEditor.Source);
            }
        }
    }
}
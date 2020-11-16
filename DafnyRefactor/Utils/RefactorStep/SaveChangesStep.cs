using System;
using System.Collections.Generic;
using System.IO;

namespace DafnyRefactor.Utils
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
            ChangesInvalidateSource = !EditsValidator.IsValid(edits, filePath);
            if (ChangesInvalidateSource) return;

            SaveTo();
        }

        protected void ApplyChanges()
        {
            var source = File.ReadAllText(filePath);
            sourceEditor = new SourceEditor(source, edits);
            sourceEditor.Apply();
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
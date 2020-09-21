using System;
using System.IO;
using DafnyRefactor;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    public class SaveChangesStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            var changer = new SaveChanges(state);
            changer.Save();

            if (changer.ChangesInvalidateSource)
            {
                state.Errors.Add("Error: refactor invalidate source");
                return;
            }

            base.Handle(state);
        }
    }

    internal class SaveChanges
    {
        protected readonly IInlineState state;
        protected SourceEditor sourceEditor;

        public SaveChanges(IInlineState state)
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
            sourceEditor = new SourceEditor(source, state.SourceEdits);
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
            if (state.Options.Stdout)
            {
                DafnyRefactorDriver.consoleOutput.Write(sourceEditor.Source);
            }
            else if (state.Options.Output != null)
            {
                File.WriteAllText(state.Options.Output, sourceEditor.Source);
            }
            else
            {
                File.WriteAllText(state.FilePath, sourceEditor.Source);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using DafnyRefactor.Utils;
using DafnyRefactor.Utils.SourceEdit;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp.Steps
{
    public class ProveImmutabilityStep : RefactorStep<InlineState>
    {
        public override void Handle(InlineState state)
        {
            var checker = new InlineImmutabilityCheck(state.FilePath, state.immutabilitySourceEdits);
            checker.Execute();
            if (!checker.IsConstant)
            {
                state.errors.Add(
                    $"Error: variable {state.inlineSymbol.Name} located on {state.inlineOptions.VarLine}:{state.inlineOptions.VarColumn} is not constant according with theorem prover.");
                return;
            }

            base.Handle(state);
        }
    }

    internal class InlineImmutabilityCheck
    {
        protected string filePath;
        protected List<SourceEdit> edits;
        public bool IsConstant { get; protected set; }

        public InlineImmutabilityCheck(string filePath, List<SourceEdit> edits)
        {
            this.filePath = filePath;
            this.edits = edits;
        }

        public void Execute()
        {
            var source = File.ReadAllText(filePath);
            var sourceEditor = new SourceEditor(source, edits);
            sourceEditor.Apply();

            var tempPath = Path.GetTempPath() + Guid.NewGuid() + ".dfy";
            File.WriteAllText(tempPath, sourceEditor.Source);

            var res = DafnyDriver.Main(new[] {tempPath, "/compile:0"});
            IsConstant = res == 0;
            File.Delete(tempPath);
        }
    }
}
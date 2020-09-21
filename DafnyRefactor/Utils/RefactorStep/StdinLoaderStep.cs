using System;
using System.IO;
using System.Text;

namespace Microsoft.DafnyRefactor.Utils
{
    public class StdinLoaderStep<TState> : RefactorStep<TState> where TState : IRefactorState
    {
        public override void Handle(TState state)
        {
            var stdinBuilder = new StringBuilder();
            string s;
            while ((s = Console.ReadLine()) != null)
            {
                stdinBuilder.Append(s);
                stdinBuilder.Append(Environment.NewLine);
            }

            var tempPath = Path.GetTempPath() + Guid.NewGuid() + ".dfy";
            File.WriteAllText(tempPath, stdinBuilder.ToString());

            state.TempFilePath = tempPath;

            base.Handle(state);
        }
    }
}
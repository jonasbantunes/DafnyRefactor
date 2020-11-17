using System;
using System.IO;
using System.Text;

namespace DafnyRefactor.Utils
{
    /// <summary>
    ///     Load source code from stdin and save to a temporary file.
    /// </summary>
    public class StdinLoaderStep<TState> : RefactorStep<TState> where TState : IRefactorState
    {
        private TState _stateRef;

        public override void Handle(TState state)
        {
            if (state == null) throw new ArgumentException();

            _stateRef = state;
            LoadFromStdin();

            base.Handle(state);
        }

        private void LoadFromStdin()
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

            _stateRef.TempFilePath = tempPath;
        }
    }
}
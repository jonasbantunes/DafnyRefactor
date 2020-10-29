using System;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    /// <summary>
    ///     A <c>RefactorStep</c> that finds the statement with informed selection.
    /// </summary>
    public class FindStatementStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.FilePath == null || state.EvUserSelection == null || state.StmtDivisors == null)
                throw new ArgumentNullException();

            var foundStmt = StmtFinder.Find(state.Program, state.EvUserSelection, state.StmtDivisors);
            if (foundStmt == null)
            {
                state.Errors.Add("Error: Couldn't find selected expression.");
                return;
            }

            state.EvStmt = foundStmt;

            base.Handle(state);
        }
    }
}
using System;
using System.Linq;

namespace DafnyRefactor.Utils
{
    /// <summary>
    ///     ParseVariables a <c>Dafny.Program</c> and generate a list of statement divisors (<c>StmtDivisors</c>).
    ///     <para>
    ///         A "divisor" is a source code position where a statement ends or where a block starts or ends.
    ///     </para>
    /// </summary>
    public class ParseStmtDivisorsStep<TState> : RefactorStep<TState> where TState : IRefactorState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null) throw new ArgumentNullException();

            var divisors = StmtDivisorsParser.Parse(state.Program);
            state.StmtDivisors = divisors.ToList();

            base.Handle(state);
        }
    }
}
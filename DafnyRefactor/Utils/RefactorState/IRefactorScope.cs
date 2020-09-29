using System.Collections.Generic;
using Microsoft.Boogie;
using Microsoft.Dafny;
using LocalVariable = Microsoft.Dafny.LocalVariable;

namespace Microsoft.DafnyRefactor.Utils
{
    /// <summary>
    ///     Represents the state of a scope.
    ///     <para>
    ///         An AST is divided in multiple scopes according to certain tokens from
    ///         statements, like from <c>BlockStmt</c> or <c>MethodDecl</c>.
    ///     </para>
    /// </summary>
    public interface IRefactorScope
    {
        IRefactorScope Parent { get; }
        IToken Token { get; }
        List<IRefactorVariable> Variables { get; }

        void InsertVariable(LocalVariable localVariable, VarDeclStmt varDeclStmt);
        void InsertScope(IToken tok);
        IRefactorVariable LookupVariable(string name);
        IRefactorScope LookupScope(int hashCode);
        IRefactorScope FindScope(int hashCode);
    }
}
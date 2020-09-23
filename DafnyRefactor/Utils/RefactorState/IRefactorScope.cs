using System.Collections.Generic;
using Microsoft.Boogie;
using Microsoft.Dafny;
using LocalVariable = Microsoft.Dafny.LocalVariable;

namespace Microsoft.DafnyRefactor.Utils
{
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
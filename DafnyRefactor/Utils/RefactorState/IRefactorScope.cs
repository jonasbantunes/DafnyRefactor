using System.Collections.Generic;
using Microsoft.Dafny;

namespace Microsoft.DafnyRefactor.Utils
{
    public interface IRefactorScope
    {
        IRefactorScope Parent { get; }
        BlockStmt BlockStmt { get; }
        List<IRefactorVariable> Variables { get; }

        void InsertVariable(LocalVariable localVariable, VarDeclStmt varDeclStmt);
        void InsertScope(BlockStmt block);
        IRefactorVariable LookupVariable(string name);
        IRefactorScope LookupScope(int hashCode);
        IRefactorScope FindScope(int hashCode);
    }
}
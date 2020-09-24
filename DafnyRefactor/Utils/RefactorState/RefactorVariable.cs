using Microsoft.Dafny;

namespace Microsoft.DafnyRefactor.Utils
{
    public interface IRefactorVariable
    {
        LocalVariable LocalVariable { get; }
        VarDeclStmt VarDeclStmt { get; }
        string Name { get; }
    }

    public class RefactorVariable : IRefactorVariable
    {
        protected readonly LocalVariable localVariable;
        protected VarDeclStmt varDeclStmt;

        public RefactorVariable(LocalVariable localVariable, VarDeclStmt varDeclStmt)
        {
            this.localVariable = localVariable;
            this.varDeclStmt = varDeclStmt;
        }

        public LocalVariable LocalVariable => localVariable;
        public VarDeclStmt VarDeclStmt => varDeclStmt;
        public string Name => LocalVariable?.Name;

        public override int GetHashCode()
        {
            return localVariable?.GetHashCode() ?? 0;
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj?.GetHashCode();
        }
    }
}
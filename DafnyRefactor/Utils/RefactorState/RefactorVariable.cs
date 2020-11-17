using Microsoft.Dafny;

namespace DafnyRefactor.Utils
{
    /// <summary>
    ///     Represents a variable from AST that is used during the refactor process.
    /// </summary>
    public interface IRefactorVariable
    {
        LocalVariable LocalVariable { get; }
        VarDeclStmt VarDeclStmt { get; }
        string Name { get; }
    }

    public class RefactorVariable : IRefactorVariable
    {
        public RefactorVariable(LocalVariable localVariable, VarDeclStmt varDeclStmt)
        {
            LocalVariable = localVariable;
            VarDeclStmt = varDeclStmt;
        }

        public LocalVariable LocalVariable { get; }
        public VarDeclStmt VarDeclStmt { get; }
        public string Name => LocalVariable?.Name;

        public override int GetHashCode()
        {
            return LocalVariable?.GetHashCode() ?? 0;
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj?.GetHashCode();
        }
    }
}
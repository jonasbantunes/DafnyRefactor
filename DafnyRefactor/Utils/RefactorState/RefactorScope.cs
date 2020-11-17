using System.Collections.Generic;
using Microsoft.Boogie;
using Microsoft.Dafny;
using LocalVariable = Microsoft.Dafny.LocalVariable;

namespace DafnyRefactor.Utils
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

    public class RefactorScope : IRefactorScope
    {
        private readonly List<IRefactorScope> _subScopes = new List<IRefactorScope>();

        public RefactorScope()
        {
        }

        public RefactorScope(IToken token = null, IRefactorScope parent = null)
        {
            Token = token;
            Parent = parent;
        }

        public IRefactorScope Parent { get; }
        public IToken Token { get; }
        public List<IRefactorVariable> Variables { get; } = new List<IRefactorVariable>();

        public void InsertVariable(LocalVariable localVariable, VarDeclStmt varDeclStmt)
        {
            var symbol = new RefactorVariable(localVariable, varDeclStmt);
            Variables.Add(symbol);
        }

        public void InsertScope(IToken tok)
        {
            var table = new RefactorScope(tok, this);
            _subScopes.Add(table);
        }

        public IRefactorVariable LookupVariable(string name)
        {
            return LookupSymbol(name, this);
        }

        public IRefactorScope LookupScope(int hashCode)
        {
            foreach (var subScope in _subScopes)
            {
                if (subScope.GetHashCode() == hashCode)
                {
                    return subScope;
                }
            }

            return null;
        }

        public IRefactorScope FindScope(int hashCode)
        {
            if (GetHashCode() == hashCode)
            {
                return this;
            }

            foreach (var subTable in _subScopes)
            {
                var result = subTable.FindScope(hashCode);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private IRefactorVariable LookupSymbol(string name, IRefactorScope scope)
        {
            foreach (var decl in scope.Variables)
            {
                if (decl.Name == name)
                {
                    return decl;
                }
            }

            if (scope.Parent == null)
            {
                return null;
            }

            return LookupSymbol(name, scope.Parent);
        }

        public override int GetHashCode()
        {
            return Token?.GetHashCode() ?? 0;
        }
    }
}
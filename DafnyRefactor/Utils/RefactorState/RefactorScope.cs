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

    public class RefactorScope : IRefactorScope
    {
        protected readonly IToken token;
        protected IRefactorScope parent;
        protected List<IRefactorScope> subScopes = new List<IRefactorScope>();
        protected List<IRefactorVariable> variables = new List<IRefactorVariable>();

        public RefactorScope()
        {
        }

        public RefactorScope(IToken token = null, IRefactorScope parent = null)
        {
            this.token = token;
            this.parent = parent;
        }

        public IRefactorScope Parent => parent;
        public IToken Token => token;
        public List<IRefactorVariable> Variables => variables;

        public void InsertVariable(LocalVariable localVariable, VarDeclStmt varDeclStmt)
        {
            var symbol = new RefactorVariable(localVariable, varDeclStmt);
            variables.Add(symbol);
        }

        public void InsertScope(IToken tok)
        {
            var table = new RefactorScope(tok, this);
            subScopes.Add(table);
        }

        public IRefactorVariable LookupVariable(string name)
        {
            return LookupSymbol(name, this);
        }

        public IRefactorScope LookupScope(int hashCode)
        {
            foreach (var subScope in subScopes)
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

            foreach (var subTable in subScopes)
            {
                var result = subTable.FindScope(hashCode);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        protected IRefactorVariable LookupSymbol(string name, IRefactorScope scope)
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
            return token?.GetHashCode() ?? 0;
        }
    }
}
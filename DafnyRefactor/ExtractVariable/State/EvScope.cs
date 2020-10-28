using System.Collections.Generic;
using Microsoft.Boogie;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;
using LocalVariable = Microsoft.Dafny.LocalVariable;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public interface IEvScope : IRefactorScope
    {
        IEvScope EvParent { get; }
    }

    public class EvScope : IEvScope
    {
        protected readonly IToken token;
        protected IEvScope parent;
        protected List<IEvScope> subScopes = new List<IEvScope>();
        protected List<IRefactorVariable> variables = new List<IRefactorVariable>();

        public EvScope()
        {
        }

        public EvScope(IToken token = null, IEvScope parent = null)
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
            var table = new EvScope(tok, this);
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

        public IEvScope EvParent => parent;

        protected IRefactorVariable LookupSymbol(string name, IEvScope scope)
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

            return LookupSymbol(name, scope.EvParent);
        }

        public override int GetHashCode()
        {
            return token?.GetHashCode() ?? 0;
        }
    }
}
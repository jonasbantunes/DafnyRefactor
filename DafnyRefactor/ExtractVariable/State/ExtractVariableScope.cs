using System.Collections.Generic;
using Microsoft.Boogie;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;
using LocalVariable = Microsoft.Dafny.LocalVariable;

namespace Microsoft.DafnyRefactor.ExtractVariable
{
    public interface IExtractVariableScope : IRefactorScope
    {
        bool CanReplace { get; set; }
        IExtractVariableScope EvrParent { get; }
        bool IsReplacable();
        IExtractVariableScope EvrLookupScope(int hashCode);
        IExtractVariableScope EvrFindScope(int hashCode);
    }

    public class ExtractVariableScope : IExtractVariableScope
    {
        private readonly List<IExtractVariableScope> _subScopes = new List<IExtractVariableScope>();

        public ExtractVariableScope()
        {
        }

        private ExtractVariableScope(IToken token = null, IExtractVariableScope parent = null)
        {
            Token = token;
            EvrParent = parent;
        }

        public IToken Token { get; }
        public List<IRefactorVariable> Variables { get; } = new List<IRefactorVariable>();
        public IRefactorScope Parent => EvrParent;
        public IExtractVariableScope EvrParent { get; }
        public bool CanReplace { get; set; }

        public void InsertVariable(LocalVariable localVariable, VarDeclStmt varDeclStmt)
        {
            var symbol = new RefactorVariable(localVariable, varDeclStmt);
            Variables.Add(symbol);
        }

        public void InsertScope(IToken tok)
        {
            var table = new ExtractVariableScope(tok, this);
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


        public bool IsReplacable()
        {
            if (CanReplace) return true;
            if (EvrParent == null) return false;
            return EvrParent.IsReplacable();
        }


        public IExtractVariableScope EvrLookupScope(int hashCode)
        {
            return (IExtractVariableScope) LookupScope(hashCode);
        }

        public IExtractVariableScope EvrFindScope(int hashCode)
        {
            return (IExtractVariableScope) FindScope(hashCode);
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
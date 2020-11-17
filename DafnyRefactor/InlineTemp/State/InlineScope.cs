using System.Collections.Generic;
using DafnyRefactor.Utils;
using Microsoft.Boogie;
using Microsoft.Dafny;
using LocalVariable = Microsoft.Dafny.LocalVariable;
using Type = Microsoft.Dafny.Type;

namespace DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     Represents the state of a scope of a "Inline Temp" refactor.
    /// </summary>
    public interface IInlineScope : IRefactorScope
    {
        IInlineScope InlineParent { get; }
        List<IInlineVariable> InlineVariables { get; }
        List<IInlineObject> InlineObjects { get; }

        IInlineVariable LookupInlineVariable(string name);
        IInlineScope FindInlineScope(int hashCode);
        IInlineScope FindScopeByVariable(IInlineVariable variable);
        void InsertInlineObject(string objPrinted, string lhsPrinted, Type objType, Type memberType);
        List<IInlineObject> GetInlineObjects();
        IRefactorMethod LookupMethod(int hashCode);
        void InsertMethod(Method mt);
    }

    public class InlineScope : IInlineScope
    {
        private readonly List<IRefactorMethod> _methods = new List<IRefactorMethod>();
        private readonly List<IInlineScope> _subScopes = new List<IInlineScope>();

        public InlineScope()
        {
        }

        private InlineScope(IToken token = null, IInlineScope parent = null)
        {
            InlineParent = parent;
            Token = token;
        }

        public IRefactorScope Parent => InlineParent;
        public IInlineScope InlineParent { get; }
        public IToken Token { get; }
        public List<IRefactorVariable> Variables => new List<IRefactorVariable>(InlineVariables);
        public List<IInlineVariable> InlineVariables { get; } = new List<IInlineVariable>();
        public List<IInlineObject> InlineObjects { get; } = new List<IInlineObject>();

        public void InsertVariable(LocalVariable localVariable, VarDeclStmt varDeclStmt)
        {
            var symbol = new InlineVariable(localVariable, varDeclStmt);
            InlineVariables.Add(symbol);
        }

        public void InsertScope(IToken tok)
        {
            var table = new InlineScope(tok, this);
            _subScopes.Add(table);
        }

        public IRefactorVariable LookupVariable(string name)
        {
            return LookupSymbol(name, this);
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

        public IInlineVariable LookupInlineVariable(string name)
        {
            return LookupVariable(name) as IInlineVariable;
        }

        public IRefactorScope LookupScope(int hashCode)
        {
            foreach (var subTable in _subScopes)
            {
                if (subTable.GetHashCode() == hashCode)
                {
                    return subTable;
                }
            }

            return null;
        }

        public IInlineScope FindInlineScope(int hashCode)
        {
            return FindScope(hashCode) as IInlineScope;
        }

        public IInlineScope FindScopeByVariable(IInlineVariable inlineVariable)
        {
            foreach (var symbol in InlineVariables)
            {
                if (symbol.GetHashCode() == inlineVariable.GetHashCode())
                {
                    return this;
                }
            }

            foreach (var subTable in _subScopes)
            {
                var result = subTable.FindScopeByVariable(inlineVariable);
                if (result != null) return result;
            }

            return null;
        }

        public void InsertInlineObject(string objPrinted, string lhsPrinted, Type objType, Type memberType)
        {
            var inlineObject = new InlineObject(objPrinted, lhsPrinted, objType, memberType);
            InlineObjects.Add(inlineObject);
        }

        public List<IInlineObject> GetInlineObjects()
        {
            return RetrieveInlineObjects(this);
        }

        public IRefactorMethod LookupMethod(int hashCode)
        {
            foreach (var method in _methods)
            {
                if (method.GetHashCode() == hashCode)
                {
                    return method;
                }
            }

            var res = InlineParent?.LookupMethod(hashCode);
            return res;
        }

        public void InsertMethod(Method mt)
        {
            var method = new RefactorMethod(mt);
            _methods.Add(method);
        }

        private List<IInlineObject> RetrieveInlineObjects(IInlineScope scope)
        {
            if (scope.Parent == null) return scope.InlineObjects;

            var objects = new List<IInlineObject>();

            var res = RetrieveInlineObjects(scope.InlineParent);
            if (res != null)
            {
                objects.AddRange(res);
            }

            if (scope.InlineObjects != null)
            {
                objects.AddRange(scope.InlineObjects);
            }

            return objects;
        }

        private IRefactorVariable LookupSymbol(string name, IInlineScope scope)
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

            return LookupSymbol(name, scope.InlineParent);
        }

        public override int GetHashCode()
        {
            return Token?.GetHashCode() ?? 0;
        }
    }
}
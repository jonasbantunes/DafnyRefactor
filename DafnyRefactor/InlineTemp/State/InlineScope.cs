using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    public interface IInlineScope : IRefactorScope
    {
        IInlineScope InlineParent { get; }
        List<IInlineVariable> InlineVariables { get; }
        List<IInlineObject> InlineObjects { get; }

        IInlineVariable LookupInlineSymbol(string name);
        IInlineScope FindInlineScope(int hashCode);
        IInlineScope FindScopeByVariable(IInlineVariable variable);
        void InsertInlineObject(string name, Type type);
        List<IInlineObject> GetInlineObjects();
    }

    public class InlineScope : IInlineScope
    {
        protected readonly BlockStmt blockStmt;
        protected List<IInlineObject> inlineObjects = new List<IInlineObject>();
        protected IInlineScope parent;
        protected List<IInlineScope> subScopes = new List<IInlineScope>();
        protected List<IInlineVariable> variables = new List<IInlineVariable>();

        public InlineScope()
        {
        }

        public InlineScope(BlockStmt blockStmt = null, IInlineScope parent = null)
        {
            this.parent = parent;
            this.blockStmt = blockStmt;
        }

        public IRefactorScope Parent => parent;
        public IInlineScope InlineParent => parent;
        public BlockStmt BlockStmt => blockStmt;
        public List<IRefactorVariable> Variables => new List<IRefactorVariable>(variables);
        public List<IInlineVariable> InlineVariables => variables;
        public List<IInlineObject> InlineObjects => inlineObjects;

        public void InsertVariable(LocalVariable localVariable, VarDeclStmt varDeclStmt)
        {
            var symbol = new InlineVariable(localVariable, varDeclStmt);
            variables.Add(symbol);
        }

        public void InsertScope(BlockStmt block)
        {
            var table = new InlineScope(block, this);
            subScopes.Add(table);
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

        public IInlineVariable LookupInlineSymbol(string name)
        {
            // TODO: Find better way to implice type
            return LookupVariable(name) as IInlineVariable;
        }

        public IRefactorScope LookupScope(int hashCode)
        {
            foreach (var subTable in subScopes)
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
            // TODO: Find better way to implice type
            return FindScope(hashCode) as IInlineScope;
        }

        public IInlineScope FindScopeByVariable(IInlineVariable inlineVariable)
        {
            foreach (var symbol in variables)
            {
                if (symbol.GetHashCode() == inlineVariable.GetHashCode())
                {
                    return this;
                }
            }

            foreach (var subTable in subScopes)
            {
                var result = subTable.FindScopeByVariable(inlineVariable);
                if (result != null) return result;
            }

            return null;
        }

        public void InsertInlineObject(string name, Type type)
        {
            var inlineObject = new InlineObject(name, type);
            inlineObjects.Add(inlineObject);
        }

        public List<IInlineObject> GetInlineObjects()
        {
            return RetrieveInlineObjects(this);
        }

        protected List<IInlineObject> RetrieveInlineObjects(IInlineScope scope)
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

        protected IRefactorVariable LookupSymbol(string name, IInlineScope scope)
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
            return blockStmt?.Tok.GetHashCode() ?? 0;
        }
    }
}
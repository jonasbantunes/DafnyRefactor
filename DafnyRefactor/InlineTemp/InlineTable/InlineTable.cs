﻿using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    public interface IInlineTable : ISymbolTable
    {
        IInlineTable InlineParent { get; }
        List<IInlineSymbol> InlineSymbols { get; }
        List<IInlineObject> InlineObjects { get; }

        IInlineSymbol LookupInlineSymbol(string name);
        IInlineTable FindInlineTable(int hashCode);
        IInlineTable FindTableBySymbol(IInlineSymbol symbol);
        void InsertInlineObject(string name, Type type);
        List<IInlineObject> GetInlineObjects();
    }

    public class InlineTable : IInlineTable
    {
        protected readonly BlockStmt blockStmt;
        protected List<IInlineObject> inlineObjects = new List<IInlineObject>();
        protected IInlineTable parent;
        protected List<IInlineTable> subTables = new List<IInlineTable>();
        protected List<IInlineSymbol> symbols = new List<IInlineSymbol>();

        public InlineTable()
        {
        }

        public InlineTable(BlockStmt blockStmt = null, IInlineTable parent = null)
        {
            this.parent = parent;
            this.blockStmt = blockStmt;
        }

        public ISymbolTable Parent => parent;
        public IInlineTable InlineParent => parent;
        public BlockStmt BlockStmt => blockStmt;
        public List<ISymbol> Symbols => new List<ISymbol>(symbols);
        public List<IInlineSymbol> InlineSymbols => symbols;
        public List<IInlineObject> InlineObjects => inlineObjects;

        public void InsertSymbol(LocalVariable localVariable, VarDeclStmt varDeclStmt)
        {
            var symbol = new InlineSymbol(localVariable, varDeclStmt);
            symbols.Add(symbol);
        }

        public void InsertTable(BlockStmt block)
        {
            var table = new InlineTable(block, this);
            subTables.Add(table);
        }

        public ISymbol LookupSymbol(string name)
        {
            return LookupSymbol(name, this);
        }

        public ISymbolTable FindTable(int hashCode)
        {
            foreach (var subTable in subTables)
            {
                if (subTable.GetHashCode() == hashCode)
                {
                    return subTable;
                }
            }

            foreach (var subTable in subTables)
            {
                var result = subTable.FindTable(hashCode);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public IInlineSymbol LookupInlineSymbol(string name)
        {
            // TODO: Find better way to implice type
            return LookupSymbol(name) as IInlineSymbol;
        }

        public ISymbolTable LookupTable(int hashCode)
        {
            foreach (var subTable in subTables)
            {
                if (subTable.GetHashCode() == hashCode)
                {
                    return subTable;
                }
            }

            return null;
        }

        public IInlineTable FindInlineTable(int hashCode)
        {
            // TODO: Find better way to implice type
            return FindTable(hashCode) as IInlineTable;
        }

        public IInlineTable FindTableBySymbol(IInlineSymbol inlineSymbol)
        {
            foreach (var symbol in symbols)
            {
                if (symbol.GetHashCode() == inlineSymbol.GetHashCode())
                {
                    return this;
                }
            }

            foreach (var subTable in subTables)
            {
                var result = subTable.FindTableBySymbol(inlineSymbol);
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

        protected List<IInlineObject> RetrieveInlineObjects(IInlineTable table)
        {
            if (table.Parent == null) return table.InlineObjects;

            var objects = new List<IInlineObject>();

            var res = RetrieveInlineObjects(table.InlineParent);
            if (res != null)
            {
                objects.AddRange(res);
            }

            if (table.InlineObjects != null)
            {
                objects.AddRange(table.InlineObjects);
            }

            return objects;
        }

        protected ISymbol LookupSymbol(string name, IInlineTable table)
        {
            foreach (var decl in table.Symbols)
            {
                if (decl.Name == name)
                {
                    return decl;
                }
            }

            if (table.Parent == null)
            {
                return null;
            }

            return LookupSymbol(name, table.InlineParent);
        }

        public override int GetHashCode()
        {
            return blockStmt?.Tok.GetHashCode() ?? 0;
        }
    }
}
using System.Collections.Generic;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils.SymbolTable
{
    // TODO: Add constructor
    public class SymbolTable<TSymbol> where TSymbol : Symbol
    {
        protected List<TSymbol> declarations = new List<TSymbol>();
        protected List<SymbolTable<TSymbol>> subTables = new List<SymbolTable<TSymbol>>();
        public readonly BlockStmt blockStmt;
        public SymbolTable<TSymbol> Parent { get; protected set; }

        public SymbolTable(BlockStmt blockStmt = null, SymbolTable<TSymbol> parent = null)
        {
            Parent = parent;
            this.blockStmt = blockStmt;
        }

        public void InsertSymbol(TSymbol declaration)
        {
            declarations.Add(declaration);
        }

        public void InsertTable(SymbolTable<TSymbol> table)
        {
            subTables.Add(table);
        }

        public TSymbol LookupSymbol(string name)
        {
            return LookupSymbol(name, this);
        }

        protected TSymbol LookupSymbol(string name, SymbolTable<TSymbol> table)
        {
            foreach (TSymbol decl in table.declarations)
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

            return LookupSymbol(name, table.Parent);
        }

        public SymbolTable<TSymbol> LookupTable(int hashCode)
        {
            foreach (SymbolTable<TSymbol> subTable in subTables)
            {
                if (subTable.GetHashCode() == hashCode)
                {
                    return subTable;
                }
            }

            return null;
        }

        public override int GetHashCode()
        {
            return blockStmt?.Tok.GetHashCode() ?? 0;
        }
    }
}
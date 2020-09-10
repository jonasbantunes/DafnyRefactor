using System.Collections.Generic;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils.SymbolTable
{
    // TODO: Add constructor
    public class SymbolTable<TSymbol> where TSymbol : Symbol
    {
        protected List<TSymbol> symbols = new List<TSymbol>();
        protected List<SymbolTable<TSymbol>> subTables = new List<SymbolTable<TSymbol>>();
        protected SymbolTable<TSymbol> parent;
        protected readonly BlockStmt blockStmt;

        public SymbolTable(BlockStmt blockStmt = null, SymbolTable<TSymbol> parent = null)
        {
            this.parent = parent;
            this.blockStmt = blockStmt;
        }

        public void InsertSymbol(TSymbol declaration)
        {
            symbols.Add(declaration);
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
            foreach (var decl in table.symbols)
            {
                if (decl.Name == name)
                {
                    return decl;
                }
            }

            if (table.parent == null)
            {
                return null;
            }

            return LookupSymbol(name, table.parent);
        }

        public SymbolTable<TSymbol> LookupTable(int hashCode)
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

        public override int GetHashCode()
        {
            return blockStmt?.Tok.GetHashCode() ?? 0;
        }
    }
}
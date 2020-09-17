using System.Collections.Generic;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils.SymbolTable
{
    public interface ISymbolTable
    {
        ISymbolTable Parent { get; }
        BlockStmt BlockStmt { get; }
        List<ISymbol> Symbols { get; }

        void InsertSymbol(LocalVariable localVariable, VarDeclStmt varDeclStmt);
        void InsertTable(BlockStmt block);
        ISymbol LookupSymbol(string name);
        ISymbolTable LookupTable(int hashCode);
        ISymbolTable FindTable(int hashCode);
    }

    public class SymbolTable : ISymbolTable
    {
        protected readonly BlockStmt blockStmt;
        protected ISymbolTable parent;
        protected List<ISymbolTable> subTables = new List<ISymbolTable>();
        protected List<ISymbol> symbols = new List<ISymbol>();

        public SymbolTable(BlockStmt blockStmt = null, ISymbolTable parent = null)
        {
            this.parent = parent;
            this.blockStmt = blockStmt;
        }

        public ISymbolTable Parent => parent;
        public BlockStmt BlockStmt => blockStmt;
        public List<ISymbol> Symbols => symbols;

        public void InsertSymbol(LocalVariable localVariable, VarDeclStmt varDeclStmt)
        {
            var symbol = new Symbol(localVariable, varDeclStmt);
            symbols.Add(symbol);
        }

        public void InsertTable(BlockStmt block)
        {
            var table = new SymbolTable(block, this);
            subTables.Add(table);
        }

        public ISymbol LookupSymbol(string name)
        {
            return LookupSymbol(name, this);
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

        protected ISymbol LookupSymbol(string name, ISymbolTable table)
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

            return LookupSymbol(name, table.Parent);
        }

        public override int GetHashCode()
        {
            return blockStmt?.Tok.GetHashCode() ?? 0;
        }
    }
}
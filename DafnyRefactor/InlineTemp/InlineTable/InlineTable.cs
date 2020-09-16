using System.Collections.Generic;
using DafnyRefactor.Utils.SymbolTable;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp.InlineTable
{
    public interface IInlineTable : ISymbolTable
    {
        IInlineTable InlineParent { get; }
        List<IInlineSymbol> InlineSymbols { get; }

        IInlineSymbol LookupInlineSymbol(string name);
        IInlineTable FindInlineTable(int hashCode);
    }

    public class InlineTable : IInlineTable
    {
        protected List<IInlineSymbol> symbols = new List<IInlineSymbol>();
        protected List<IInlineTable> subTables = new List<IInlineTable>();
        protected IInlineTable parent;
        protected readonly BlockStmt blockStmt;

        public ISymbolTable Parent => parent;
        public IInlineTable InlineParent => parent;
        public BlockStmt BlockStmt => blockStmt;
        public List<ISymbol> Symbols => new List<ISymbol>(symbols);
        public List<IInlineSymbol> InlineSymbols => symbols;

        public InlineTable()
        {
        }

        public InlineTable(BlockStmt blockStmt = null, IInlineTable parent = null)
        {
            this.parent = parent;
            this.blockStmt = blockStmt;
        }

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
            throw new System.NotImplementedException();
        }

        public IInlineTable FindInlineTable(int hashCode)
        {
            // TODO: Find better way to implice type
            return FindTable(hashCode) as IInlineTable;
        }

        public override int GetHashCode()
        {
            return blockStmt?.Tok.GetHashCode() ?? 0;
        }
    }
}
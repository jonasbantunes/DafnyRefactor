using System.Collections.Generic;

namespace Microsoft.Dafny
{
    // TODO: Add constructor
    public class SymbolTable
    {
        protected List<SymbolTableDeclaration> declarations = new List<SymbolTableDeclaration>();
        protected List<SymbolTable> subTables = new List<SymbolTable>();
        public readonly BlockStmt blockStmt;
        public SymbolTable Parent { get; protected set; }

        public SymbolTable(BlockStmt blockStmt = null, SymbolTable parent = null)
        {
            Parent = parent;
            this.blockStmt = blockStmt;
        }

        public void InsertDeclaration(SymbolTableDeclaration declaration)
        {
            declarations.Add(declaration);
        }

        public void InsertTable(SymbolTable table)
        {
            subTables.Add(table);
        }

        public SymbolTableDeclaration LookupDeclaration(string name)
        {
            return LookupDeclaration(name, this);
        }

        protected SymbolTableDeclaration LookupDeclaration(string name, SymbolTable table)
        {
            foreach (SymbolTableDeclaration decl in table.declarations)
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

            return LookupDeclaration(name, table.Parent);
        }

        public SymbolTable LookupTable(int hashCode)
        {
            foreach (SymbolTable subTable in subTables)
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

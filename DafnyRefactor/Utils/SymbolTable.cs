﻿using Microsoft.Boogie;
using System.Collections.Generic;

namespace Microsoft.Dafny
{
    public class SymbolTable
    {
        protected List<SymbolTableDeclaration> declarations = new List<SymbolTableDeclaration>();
        protected List<SymbolTable> subTables = new List<SymbolTable>();
        public int hashCode;
        public SymbolTable parent = null;

        public void Insert(IToken tok)
        {
            var declaration = new SymbolTableDeclaration
            {
                name = tok.val,
                hashCode = tok.GetHashCode()
            };
            declarations.Add(declaration);
        }

        public void Insert(SymbolTable table)
        {
            subTables.Add(table);
        }

        public SymbolTableDeclaration Lookup(string name)
        {
            return Lookup(name, this);
        }

        public SymbolTableDeclaration Lookup(string name, SymbolTable table)
        {
            foreach (SymbolTableDeclaration decl in table.declarations)
            {
                if (decl.name == name)
                {
                    return decl;
                }
            }

            if (table.parent == null)
            {
                return null;
            }

            return Lookup(name, table.parent);
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
            return hashCode;
        }

    }
}

using System.Collections.Generic;
using Microsoft.Dafny;

namespace Microsoft.DafnyRefactor.Utils
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
}
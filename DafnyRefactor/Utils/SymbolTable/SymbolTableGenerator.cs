using System;
using DafnyRefactor.Utils.DafnyVisitor;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils.SymbolTable
{
    public class SymbolTableGenerator<TSymbol> : DafnyWithTableVisitor<TSymbol> where TSymbol : Symbol
    {
        protected Func<LocalVariable, VarDeclStmt, TSymbol> symbolCreatorFunc;
        public SymbolTable<TSymbol> GeneratedTable { get; protected set; }

        public SymbolTableGenerator(Program program, Func<LocalVariable, VarDeclStmt, TSymbol> symbolCreatorFunc) :
            base(program, null)
        {
            this.symbolCreatorFunc = symbolCreatorFunc;
        }

        public override void Execute()
        {
            GeneratedTable = new SymbolTable<TSymbol>();
            rootTable = GeneratedTable;
            curTable = GeneratedTable;
            base.Execute();
        }

        protected override void Visit(VarDeclStmt vds)
        {
            foreach (LocalVariable local in vds.Locals)
            {
                // TODO: Change to lambda function
                // var declaration = new Symbol(local, vds);
                // curTable.InsertSymbol(declaration as TSymbol);
                var declaration = symbolCreatorFunc(local, vds);
                curTable.InsertSymbol(declaration);
            }
        }

        protected override void Visit(BlockStmt block)
        {
            var subTable = new SymbolTable<TSymbol>(block, curTable);
            curTable.InsertTable(subTable);

            base.Visit(block);
        }
    }
}
using System;

namespace Microsoft.Dafny
{
    public class LocateVariableStep : DafnyVisitor
    {
        protected SymbolTable curTable;
        public SymbolTable table { protected get; set; }
        public int varLine { protected get; set; }
        public int varColumn { protected get; set; }
        public SymbolTableDeclaration found { get; protected set; }

        public override void execute()
        {
            curTable = table;
            next(program);
        }

        protected override WhileStmt next(WhileStmt while_)
        {
            curTable = curTable.lookupTable(while_.Tok.GetHashCode());

            foreach (Statement stmt in while_.Body.Body)
            {
                next(stmt);
            }

            curTable = curTable.parent;

            return while_;
        }

        protected override VarDeclStmt next(VarDeclStmt vds)
        {
            foreach (var local in vds.Locals)
            {
                if (isInRange(varLine, varColumn, local.Tok.line, local.Tok.col, local.EndTok.line, local.EndTok.col))
                {
                    found = curTable.lookup(local.Name);
                }
            }

            return vds;
        }

        protected bool isInRange(int line, int column, int startLine, int starColumn, int endLine, int endColumn)
        {
            if (startLine == line && starColumn <= column)
            {
                if (startLine < line && line < endLine || (line == endLine && column <= endColumn))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

﻿namespace Microsoft.Dafny
{
    public class LocateVariableStep : DafnyWithTableVisitor
    {
        // TODO: Analyse if varLine and varColumn should be an Location "struct"
        protected int varLine;
        protected int varColumn;
        public SymbolTableDeclaration FoundDeclaration { get; protected set; }

        public LocateVariableStep(Program program, SymbolTable rootTable, int varLine, int varColumn) : base(program, rootTable)
        {
            this.varLine = varLine;
            this.varColumn = varColumn;
        }

        public override void Execute()
        {
            curTable = rootTable;
            base.Execute();
        }

        protected override VarDeclStmt Visit(VarDeclStmt vds)
        {
            foreach (var local in vds.Locals)
            {
                if (IsInRange(varLine, varColumn, local.Tok.line, local.Tok.col, local.EndTok.line, local.EndTok.col))
                {
                    FoundDeclaration = curTable.LookupDeclaration(local.Name);
                }
            }

            return base.Visit(vds);
        }

        protected override NameSegment Visit(NameSegment nameSeg)
        {
            if (IsInRange(varLine, varColumn, nameSeg.tok.line, nameSeg.tok.col, nameSeg.tok.line, nameSeg.tok.col + nameSeg.tok.val.Length - 1))
            {
                FoundDeclaration = curTable.LookupDeclaration(nameSeg.Name);
            }
            return nameSeg;
        }

        protected bool IsInRange(int line, int column, int startLine, int starColumn, int endLine, int endColumn)
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

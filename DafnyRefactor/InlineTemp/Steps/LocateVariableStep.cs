using DafnyRefactor.InlineTemp.InlineTable;
using DafnyRefactor.Utils;
using DafnyRefactor.Utils.DafnyVisitor;
using DafnyRefactor.Utils.SymbolTable;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp.Steps
{
    public class LocateVariableStep : RefactorStep<InlineState>
    {
        public override void Handle(InlineState state)
        {
            var visitor = new LocateVariableVisitor(state.program, state.symbolTable, state.inlineOptions.VarLine,
                state.inlineOptions.VarColumn);
            visitor.Execute();
            if (visitor.FoundDeclaration == null)
            {
                state.errors.Add(
                    $"Error: can't locate variable on line {state.inlineOptions.VarLine}:{state.inlineOptions.VarColumn}.");
                return;
            }

            state.inlineSymbol = visitor.FoundDeclaration;

            base.Handle(state);
        }
    }

    internal class LocateVariableVisitor : DafnyWithTableVisitor<InlineSymbol>
    {
        // TODO: Analyse if varLine and varColumn should be an Location "struct"
        protected int varLine;
        protected int varColumn;
        public InlineSymbol FoundDeclaration { get; protected set; }

        public LocateVariableVisitor(Program program, SymbolTable<InlineSymbol> rootTable, int varLine, int varColumn) :
            base(
                program,
                rootTable)
        {
            this.varLine = varLine;
            this.varColumn = varColumn;
        }

        protected override void Visit(VarDeclStmt vds)
        {
            foreach (var local in vds.Locals)
            {
                if (IsInRange(varLine, varColumn, local.Tok.line, local.Tok.col, local.EndTok.line, local.EndTok.col))
                {
                    FoundDeclaration = curTable.LookupSymbol(local.Name);
                }
            }

            base.Visit(vds);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            if (IsInRange(varLine, varColumn, nameSeg.tok.line, nameSeg.tok.col, nameSeg.tok.line,
                nameSeg.tok.col + nameSeg.tok.val.Length - 1))
            {
                FoundDeclaration = curTable.LookupSymbol(nameSeg.Name);
            }
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
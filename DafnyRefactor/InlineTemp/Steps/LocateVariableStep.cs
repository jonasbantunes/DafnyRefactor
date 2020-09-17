using DafnyRefactor.InlineTemp.InlineTable;
using DafnyRefactor.Utils;
using DafnyRefactor.Utils.DafnyVisitor;
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

    internal class LocateVariableVisitor : DafnyVisitor
    {
        protected IInlineTable rootTable;

        protected int varColumn;

        // TODO: Analyse if varLine and varColumn should be an Location "struct"
        protected int varLine;

        public LocateVariableVisitor(Program program, IInlineTable rootTable, int varLine, int varColumn) :
            base(program)
        {
            this.varLine = varLine;
            this.varColumn = varColumn;
            this.rootTable = rootTable;
        }

        public IInlineSymbol FoundDeclaration { get; protected set; }

        protected override void Visit(VarDeclStmt vds)
        {
            foreach (var local in vds.Locals)
            {
                if (IsInRange(varLine, varColumn, local.Tok.line, local.Tok.col, local.EndTok.line, local.EndTok.col))
                {
                    // TODO: Avoid this repetition on source code
                    var curTable = rootTable.FindInlineTable(nearestBlockStmt.Tok.GetHashCode());
                    FoundDeclaration = curTable.LookupInlineSymbol(local.Name);
                }
            }

            base.Visit(vds);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            if (IsInRange(varLine, varColumn, nameSeg.tok.line, nameSeg.tok.col, nameSeg.tok.line,
                nameSeg.tok.col + nameSeg.tok.val.Length - 1))
            {
                // TODO: Avoid this repetition on source code
                var curTable = rootTable.FindInlineTable(nearestBlockStmt.Tok.GetHashCode());
                FoundDeclaration = curTable.LookupInlineSymbol(nameSeg.Name);
            }
        }

        protected bool IsInRange(int line, int column, int startLine, int starColumn, int endLine, int endColumn)
        {
            if (startLine == line && starColumn <= column)
            {
                if (startLine < line && line < endLine || line == endLine && column <= endColumn)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
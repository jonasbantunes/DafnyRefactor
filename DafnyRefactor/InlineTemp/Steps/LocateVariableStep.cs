using System;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    public class LocateVariableStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Errors == null || state.InlineOptions == null || state.Program == null ||
                state.RootScope == null) throw new ArgumentNullException();

            var visitor = new LocateVariableVisitor(state.Program, state.RootScope, state.InlineOptions.VarLine,
                state.InlineOptions.VarColumn);
            visitor.Execute();
            if (visitor.FoundDeclaration == null)
            {
                state.AddError(
                    $"Error: can't locate variable on line {state.InlineOptions.VarLine}:{state.InlineOptions.VarColumn}.");
                return;
            }

            state.InlineVariable = visitor.FoundDeclaration;

            base.Handle(state);
        }
    }

    internal class LocateVariableVisitor : DafnyVisitor
    {
        protected Program program;
        protected IInlineScope rootScope;

        protected int varColumn;

        // TODO: Analyse if varLine and varColumn should be an Location "struct"
        protected int varLine;

        public LocateVariableVisitor(Program program, IInlineScope rootScope, int varLine, int varColumn)
        {
            if (program == null || rootScope == null) throw new ArgumentNullException();

            this.program = program;
            this.varLine = varLine;
            this.varColumn = varColumn;
            this.rootScope = rootScope;
        }

        public IInlineVariable FoundDeclaration { get; protected set; }

        public virtual void Execute()
        {
            Visit(program);
        }

        protected override void Visit(VarDeclStmt vds)
        {
            if (vds == null) throw new ArgumentNullException();

            foreach (var local in vds.Locals)
            {
                if (IsInRange(varLine, varColumn, local.Tok.line, local.Tok.col, local.EndTok.line, local.EndTok.col))
                {
                    // TODO: Avoid this repetition on source code
                    var curTable = rootScope.FindInlineScope(nearestBlockStmt.Tok.GetHashCode());
                    FoundDeclaration = curTable.LookupInlineSymbol(local.Name);
                }
            }

            base.Visit(vds);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            if (nameSeg == null) throw new ArgumentNullException();

            if (IsInRange(varLine, varColumn, nameSeg.tok.line, nameSeg.tok.col, nameSeg.tok.line,
                nameSeg.tok.col + nameSeg.tok.val.Length - 1))
            {
                // TODO: Avoid this repetition on source code
                var curTable = rootScope.FindInlineScope(nearestBlockStmt.Tok.GetHashCode());
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
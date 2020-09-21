using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    public class ProveImmutabilityClassicStep<TInlineState> : RefactorStep<TInlineState>
        where TInlineState : IInlineState
    {
        public override void Handle(TInlineState state)
        {
            var parser = new ParseInlineSymbolExpr(state.InlineSymbol, state.SymbolTable);
            parser.Execute();
            var table = state.SymbolTable.FindTableBySymbol(state.InlineSymbol);
            foreach (var inlineObject in parser.InlineObjects)
            {
                table.InsertInlineObject(inlineObject.Name, inlineObject.Type);
            }

            var assertives = new AddAssertivesClassic(state.Program, state.StmtDivisors, state.SymbolTable,
                state.InlineSymbol);
            assertives.Execute();

            var checker = new InlineImmutabilityCheckClassic(state.FilePath, assertives.Edits);
            checker.Execute();

            if (!checker.IsConstant)
            {
                // TODO: Don't access list directly
                state.Errors.Add(
                    $"Error: variable {state.InlineSymbol.Name} located on {state.InlineOptions.VarLine}:{state.InlineOptions.VarColumn} is not constant according with theorem prover.");
                return;
            }

            base.Handle(state);
        }
    }

    internal class ParseInlineSymbolExpr : DafnyVisitor
    {
        protected IInlineSymbol inlineSymbol;
        protected IInlineTable inlineTable;

        public ParseInlineSymbolExpr(IInlineSymbol inlineSymbol, IInlineTable inlineTable)
        {
            this.inlineSymbol = inlineSymbol;
            this.inlineTable = inlineTable;
        }

        public List<InlineObject> InlineObjects { get; protected set; } = new List<InlineObject>();

        public override void Execute()
        {
            Visit(inlineSymbol.Expr);
        }

        protected override void Visit(Expression exp)
        {
            base.Visit(exp);
            Traverse(exp.SubExpressions.ToList());
        }

        protected override void Visit(ExprDotName exprDotName)
        {
            var name = Printer.ExprToString(exprDotName);
            var type = exprDotName.Type;
            InlineObjects.Add(new InlineObject(name, type));
            base.Visit(exprDotName);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            var name = Printer.ExprToString(nameSeg);
            var type = nameSeg.Type;
            InlineObjects.Add(new InlineObject(name, type));
            base.Visit(nameSeg);
        }
    }

    internal class AddAssertivesClassic : DafnyVisitor
    {
        protected IInlineSymbol inlineSymbol;
        protected IInlineTable inlineTable;
        protected Statement nearestStmt;
        protected List<int> stmtDivisors;

        public AddAssertivesClassic(Program program, List<int> stmtDivisors, IInlineTable inlineTable,
            IInlineSymbol inlineSymbol) : base(program)
        {
            this.stmtDivisors = stmtDivisors;
            this.inlineTable = inlineTable;
            this.inlineSymbol = inlineSymbol;
        }

        public List<SourceEdit> Edits { get; protected set; }

        public override void Execute()
        {
            Edits = new List<SourceEdit>();
            base.Execute();
        }

        protected override void Visit(Statement stmt)
        {
            var oldNearestStmt = nearestStmt;
            nearestStmt = stmt;
            base.Visit(stmt);
            nearestStmt = oldNearestStmt;
        }

        protected override void Visit(UpdateStmt up)
        {
            Traverse(up.Lhss);
        }

        protected override void Visit(ExprDotName exprDotName)
        {
            if (nearestStmt.Tok.pos < inlineSymbol.InitStmt.EndTok.pos) return;
            if (nearestStmt is AssignStmt || nearestStmt is UpdateStmt)
            {
                var findIndex = stmtDivisors.FindIndex(divisor => divisor >= nearestStmt.EndTok.pos);
                if (findIndex <= 1) return;

                var curTable = inlineTable.FindInlineTable(nearestBlockStmt.Tok.GetHashCode());
                if (curTable == null) return;

                var assertives = new List<string>();
                foreach (var inlineObject in curTable.InlineObjects)
                {
                    if (!exprDotName.Lhs.Type.Equals(inlineObject.Type)) continue;
                    var assertStmtExpr = $"\n assert {Printer.ExprToString(exprDotName.Lhs)} != {inlineObject.Name};\n";
                    Edits.Add(new SourceEdit(stmtDivisors[findIndex - 1] + 1, assertStmtExpr));
                }
            }

            base.Visit(exprDotName);
        }
    }

    internal class InlineImmutabilityCheckClassic
    {
        protected List<SourceEdit> edits;
        protected string filePath;

        public InlineImmutabilityCheckClassic(string filePath, List<SourceEdit> edits)
        {
            this.filePath = filePath;
            this.edits = edits;
        }

        public bool IsConstant { get; protected set; }

        public void Execute()
        {
            var source = File.ReadAllText(filePath);
            var sourceEditor = new SourceEditor(source, edits);
            sourceEditor.Apply();

            var tempPath = Path.GetTempPath() + Guid.NewGuid() + ".dfy";
            File.WriteAllText(tempPath, sourceEditor.Source);

            var res = DafnyDriver.Main(new[] {tempPath, "/compile:0"});
            IsConstant = res == 0;
            File.Delete(tempPath);
        }
    }
}
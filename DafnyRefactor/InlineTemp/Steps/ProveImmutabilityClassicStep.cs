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
            if (state == null || state.Errors == null || state.FilePath == null || state.InlineOptions == null ||
                state.InlineVariable == null || state.Program == null || state.StmtDivisors == null ||
                state.RootScope == null) throw new ArgumentNullException();

            var parser = new ParseInlineSymbolExpr(state.InlineVariable, state.RootScope);
            parser.Execute();
            var table = state.RootScope.FindScopeByVariable(state.InlineVariable);
            foreach (var inlineObject in parser.InlineObjects)
            {
                table.InsertInlineObject(inlineObject.Name, inlineObject.Type);
            }

            var assertives = new AddAssertivesClassic(state.Program, state.StmtDivisors, state.RootScope,
                state.InlineVariable);
            assertives.Execute();

            var checker = new InlineImmutabilityCheckClassic(state.FilePath, assertives.Edits);
            checker.Execute();

            if (!checker.IsConstant)
            {
                state.AddError(
                    $"Error: variable {state.InlineVariable.Name} located on {state.InlineOptions.VarLine}:{state.InlineOptions.VarColumn} is not constant according with theorem prover.");
                return;
            }

            base.Handle(state);
        }
    }

    internal class ParseInlineSymbolExpr : DafnyVisitor
    {
        protected IInlineVariable inlineVariable;
        protected IInlineScope inlineScope;

        public ParseInlineSymbolExpr(IInlineVariable inlineVariable, IInlineScope inlineScope)
        {
            if (inlineVariable == null || inlineScope == null) throw new ArgumentNullException();

            this.inlineVariable = inlineVariable;
            this.inlineScope = inlineScope;
        }

        public List<InlineObject> InlineObjects { get; protected set; } = new List<InlineObject>();

        public virtual void Execute()
        {
            Visit(inlineVariable.Expr);
        }

        protected override void Visit(ExprDotName exprDotName)
        {
            if (exprDotName == null) throw new ArgumentNullException();

            var name = Printer.ExprToString(exprDotName);
            var type = exprDotName.Type;
            InlineObjects.Add(new InlineObject(name, type));
            base.Visit(exprDotName);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            if (nameSeg == null) throw new ArgumentNullException();

            var name = Printer.ExprToString(nameSeg);
            var type = nameSeg.Type;
            InlineObjects.Add(new InlineObject(name, type));
            base.Visit(nameSeg);
        }
    }

    internal class AddAssertivesClassic : DafnyVisitorWithNearests
    {
        protected IInlineVariable inlineVariable;
        protected IInlineScope rootScope;
        protected Program program;
        protected List<int> stmtDivisors;

        public AddAssertivesClassic(Program program, List<int> stmtDivisors, IInlineScope rootScope,
            IInlineVariable inlineVariable)
        {
            if (rootScope == null || program == null || stmtDivisors == null || inlineVariable == null)
                throw new ArgumentNullException();

            this.program = program;
            this.stmtDivisors = stmtDivisors;
            this.rootScope = rootScope;
            this.inlineVariable = inlineVariable;
        }

        public List<SourceEdit> Edits { get; protected set; }

        public virtual void Execute()
        {
            Edits = new List<SourceEdit>();
            Visit(program);
        }

        protected override void Visit(AssignStmt assignStmt)
        {
            if (assignStmt == null) throw new ArgumentNullException();
            if (nearestStmt.Tok.pos < inlineVariable.InitStmt.EndTok.pos) return;

            if (assignStmt.Lhs is MemberSelectExpr memberSelectExpr)
            {
                var findIndex = stmtDivisors.FindIndex(divisor => divisor >= nearestStmt.EndTok.pos);
                if (findIndex <= 1) return;

                var curTable = rootScope.FindInlineScope(nearestBlockStmt.Tok.GetHashCode());
                if (curTable == null) return;

                foreach (var inlineObject in curTable.GetInlineObjects())
                {
                    if (!memberSelectExpr.Obj.Type.Equals(inlineObject.Type)) continue;
                    var assertStmtExpr =
                        $"\n assert {Printer.ExprToString(memberSelectExpr.Obj)} != {inlineObject.Name};\n";
                    Edits.Add(new SourceEdit(stmtDivisors[findIndex - 1] + 1, assertStmtExpr));
                }
            }

            base.Visit(assignStmt);
        }

        protected override void Visit(CallStmt callStmt)
        {
            if (callStmt == null) throw new ArgumentNullException();

            var findIndex = stmtDivisors.FindIndex(divisor => divisor >= nearestStmt.EndTok.pos);
            if (findIndex <= 1) return;

            var curTable = rootScope.FindInlineScope(nearestBlockStmt.Tok.GetHashCode());
            if (curTable == null) return;

            var method = curTable.LookupMethod(callStmt.Method.GetHashCode());
            if (method == null) return;

            for (var i = 0; i < callStmt.Args.Count; i++)
            {
                var arg = callStmt.Args[i];
                var methodArg = method.Args[i];

                if (methodArg.CanBeModified)
                {
                    foreach (var inlineObject in curTable.GetInlineObjects())
                    {
                        if (methodArg.Type.Equals(inlineObject.Type))
                        {
                            var assertStmtExpr =
                                $"\n assert {Printer.ExprToString(arg)} != {inlineObject.Name};\n";
                            Edits.Add(new SourceEdit(stmtDivisors[findIndex - 1] + 1, assertStmtExpr));
                        }
                    }
                }
            }

            base.Visit(callStmt);
        }

        //protected override void Visit(NameSegment nameSeg)
        //{
        //    if (nameSeg == null) throw new ArgumentNullException();

        //    if (nearestStmt is CallStmt callStmt)
        //    {
        //        var findIndex = stmtDivisors.FindIndex(divisor => divisor >= nearestStmt.EndTok.pos);
        //        if (findIndex <= 1) return;

        //        var curTable = rootScope.FindInlineScope(nearestBlockStmt.Tok.GetHashCode());
        //        if (curTable == null) return;

        //        var method = curTable.LookupMethod(callStmt.Method.GetHashCode());
        //        if (method == null) return;

        //        var arg = method.LookupArg(nameSeg.Name);
        //        if (arg == null || !arg.CanBeModified) return;

        //        foreach (var inlineObject in curTable.GetInlineObjects())
        //        {
        //            if (nameSeg.Resolved.Type.Equals(inlineObject.Type))
        //            {
        //                var assertStmtExpr =
        //                    $"\n assert {nameSeg.Name} != {inlineObject.Name};\n";
        //                Edits.Add(new SourceEdit(stmtDivisors[findIndex - 1] + 1, assertStmtExpr));
        //            }
        //        }
        //    }

        //    base.Visit(nameSeg);
        //}
    }

    internal class InlineImmutabilityCheckClassic
    {
        protected List<SourceEdit> edits;
        protected string filePath;

        public InlineImmutabilityCheckClassic(string filePath, List<SourceEdit> edits)
        {
            if (filePath == null || edits == null) throw new ArgumentNullException();

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
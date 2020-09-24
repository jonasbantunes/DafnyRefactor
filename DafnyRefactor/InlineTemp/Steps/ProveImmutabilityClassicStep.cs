using System;
using System.Collections.Generic;
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
            var scope = state.RootScope.FindScopeByVariable(state.InlineVariable);
            foreach (var inlineObject in parser.InlineObjects)
            {
                scope.InsertInlineObject(inlineObject.Name, inlineObject.Type);
            }

            var assertives = new AddAssertivesClassic(state.Program, state.StmtDivisors, state.RootScope,
                state.InlineVariable);
            assertives.Execute();

            var checker = new InlineImmutabilityCheck(state.FilePath, assertives.Edits);
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

    public class ParseInlineSymbolExpr : DafnyVisitor
    {
        protected IInlineScope inlineScope;
        protected IInlineVariable inlineVariable;

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

    public class AddAssertivesClassic : DafnyVisitorWithNearests
    {
        protected IInlineVariable inlineVariable;
        protected Program program;
        protected IInlineScope rootScope;
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

        protected IInlineScope CurScope => rootScope.FindInlineScope(nearestScopeToken.GetHashCode());
        public List<SourceEdit> Edits { get; protected set; }

        public virtual void Execute()
        {
            Edits = new List<SourceEdit>();
            Visit(program);
        }

        protected override void Visit(AssignStmt assignStmt)
        {
            if (nearestStmt.Tok.pos < inlineVariable.InitStmt.EndTok.pos) return;
            if (!(assignStmt.Lhs is MemberSelectExpr memberSelectExpr)) return;

            var findIndex = stmtDivisors.FindIndex(divisor => divisor >= nearestStmt.EndTok.pos);
            if (findIndex <= 1) return;

            if (CurScope == null) return;
            foreach (var inlineObject in CurScope.GetInlineObjects())
            {
                var obj = memberSelectExpr.Obj;
                if (!obj.Type.Equals(inlineObject.Type)) continue;

                var assertStmtExpr = $"\n assert {Printer.ExprToString(obj)} != {inlineObject.Name};";
                var pos = stmtDivisors[findIndex - 1] + 1;
                var edit = new SourceEdit(pos, assertStmtExpr);
                Edits.Add(edit);
            }
        }

        protected override void Visit(CallStmt callStmt)
        {
            var divisorIndex = stmtDivisors.FindIndex(divisor => divisor >= nearestStmt.EndTok.pos);
            if (divisorIndex <= 1) return;

            var method = CurScope?.LookupMethod(callStmt.Method.GetHashCode());
            if (method == null) return;

            for (var i = 0; i < callStmt.Args.Count; i++)
            {
                var arg = callStmt.Args[i];
                var methodArg = method.Args[i];

                if (!methodArg.CanBeModified) continue;
                foreach (var inlineObject in CurScope.GetInlineObjects())
                {
                    if (!methodArg.Type.Equals(inlineObject.Type)) continue;

                    var argPrinted = Printer.ExprToString(arg);
                    var assertStmtExpr = $"\n assert {argPrinted} != {inlineObject.Name};";

                    var pos = stmtDivisors[divisorIndex - 1] + 1;
                    var edit = new SourceEdit(pos, assertStmtExpr);
                    Edits.Add(edit);
                }
            }
        }
    }
}
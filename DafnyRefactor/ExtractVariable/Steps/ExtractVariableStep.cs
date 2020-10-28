using System;
using System.Collections.Generic;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.ExtractVariable;
using Microsoft.DafnyRefactor.Utils;

namespace DafnyRefactor.ExtractVariable.Steps
{
    /// <summary>
    ///     A <c>RefactorStep</c> that extracts the selected expression from <c>state.EvStmt</c>.
    /// </summary>
    public class ExtractVariableStep<TState> : RefactorStep<TState> where TState : IExtractVariableState
    {
        protected TState inState;

        public override void Handle(TState state)
        {
            if (state == null || state.EvExprRange == null || state.EvStmt == null ||
                state.EvOptions == null || state.Program == null || state.EvSourceCode == null ||
                state.StmtDivisors == null || state.EvRootScope == null)
                throw new ArgumentNullException();

            inState = state;

            Extract();
            Parse();
            Validate();
            if (state.Errors.Count > 0) return;

            base.Handle(state);
        }

        protected void Extract()
        {
            var exprStart = inState.EvExprRange.start;
            var exprEnd = inState.EvExprRange.end;
            var exprRaw = inState.EvSourceCode.Substring(exprStart, exprEnd - exprStart).Trim();

            var varName = inState.EvOptions.VarName;
            var editRaw = $"\nvar {varName} := {exprRaw};";

            var divisorIndex =
                inState.StmtDivisors.FindIndex(divisor => divisor >= inState.EvStmt.EndTok.pos);
            var editPos = inState.StmtDivisors[divisorIndex - 1] + 1;
            var edit = new SourceEdit(editPos, editRaw);
            inState.SourceEdits.Add(edit);
        }

        protected void Parse()
        {
            var parser = new EvParseVisitor(inState.Program, inState.EvExprRange, inState.EvRootScope);
            parser.Execute();
            inState.EvExprVariables.AddRange(parser.Variables);
        }

        protected void Validate()
        {
            var validator = new EditsValidator(inState.FilePath, inState.SourceEdits);
            validator.Execute();
            if (!validator.IsValid)
            {
                inState.Errors.Add("Error: Selected expression is invalid");
            }
        }
    }

    internal class EvParseVisitor : DafnyVisitorWithNearests
    {
        protected Program program;
        protected Range expRange;
        protected IEvScope rootScope;
        public List<IRefactorVariable> Variables { get; protected set; }

        public EvParseVisitor(Program program, Range expRange, IEvScope rootScope)
        {
            this.program = program;
            this.expRange = expRange;
            this.rootScope = rootScope;
        }

        public void Execute()
        {
            Variables = new List<IRefactorVariable>();
            Visit(program);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            if (expRange.start > nameSeg.tok.pos || nameSeg.tok.pos > expRange.end) return;

            var curScope = rootScope.FindScope(nearestScopeToken.GetHashCode());
            if (curScope == null) return;

            var variable = curScope.LookupVariable(nameSeg.Name);
            if (variable == null) return;

            Variables.Add(variable);

            base.Visit(nameSeg);
        }
    }
}
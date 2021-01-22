using System;
using DafnyRefactor.Utils;

namespace DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     A <c>RefactorStep</c> that verifies if all usages of a <c>InlineVariable</c> are constant.
    /// </summary>
    public class AssertImmutabilityStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Errors == null || state.FilePath == null || state.InlineOptions == null ||
                state.InlineVariable == null || state.Program == null || state.StmtDivisors == null ||
                state.RootScope == null) throw new ArgumentNullException();

            var inlineObjects = InlineObjectsParser.Parse(state.InlineVariable);
            var scope = state.RootScope.FindScopeByVariable(state.InlineVariable);
            foreach (var inlineObject in inlineObjects)
            {
                scope.InsertInlineObject(inlineObject.ObjPrinted, inlineObject.LhsPrinted, inlineObject.ObjType,
                    inlineObject.MemberType);
            }

            var assertiveEdits = AssertivesAdder.GetAssertives(state.Program, state.StmtDivisors, state.RootScope,
                state.InlineVariable);

            var isValid = EditsValidator.IsValid(assertiveEdits, state.FilePath);
            if (!isValid)
            {
                state.AddError(InlineTempErrorMsg.NotConstantBySolver(state.InlineVariable.Name,
                    state.InlineOptions.Position));
                return;
            }

            base.Handle(state);
        }
    }
}
using System;
using DafnyRefactor.Utils;

namespace DafnyRefactor.MoveMethodToAssociated
{
    public class ParsePositionStep<TState> : RefactorStep<TState> where TState : IMoveToAssociatedState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Options == null || state.SourceCode == null) throw new ArgumentNullException();

            var originRaw = state.MtaOptions.MethodPosition.Split(':');
            var targetRaw = state.MtaOptions.FieldPosition.Split(':');
            if (originRaw.Length != 2 || targetRaw.Length != 2)
            {
                state.AddError(MoveToAssociatedErrors.WrongPositionSyntax());
                return;
            }

            var originLine = int.Parse(originRaw[0]);
            var originCol = int.Parse(originRaw[1]);
            var targetLine = int.Parse(targetRaw[0]);
            var targetCol = int.Parse(targetRaw[1]);

            var originIndex = 0;
            var targetIndex = 0;
            if (originLine - 1 > 0)
            {
                originIndex = state.SourceCode.IndexOfNth("\n", originLine - 1);
            }
            if (targetLine - 1 > 0)
            {
                targetIndex = state.SourceCode.IndexOfNth("\n", targetLine - 1);
            }

            if (originIndex == -1 || targetIndex == -1)
            {
                state.AddError(MoveToAssociatedErrors.InvalidPosition());
                return;
            }

            state.MtaOriginPos = originIndex + originCol;
            state.MtaTargetPos = targetIndex + targetCol;

            base.Handle(state);
        }
    }
}
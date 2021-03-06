﻿using System;

namespace DafnyRefactor.Utils
{
    /// <summary>
    ///     Base class representing a step from a refactor, using a similar approach to "Chain of Responsability" pattern.
    ///     <para>
    ///         Each refactor is divided in multiple substeps that share a common state <c>RefactorState</c>.
    ///     </para>
    /// </summary>
    public abstract class RefactorStep<TRefactorState> where TRefactorState : IRefactorState
    {
        public RefactorStep<TRefactorState> next;

        public virtual void Handle(TRefactorState state)
        {
            if (state == null || state.Errors == null) throw new ArgumentException();

            if (state.Errors.Count > 0) return;
            next?.Handle(state);
        }
    }
}
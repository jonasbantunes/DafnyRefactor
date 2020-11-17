using System;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp
{
    /// <summary>
    ///     A <c>RefactorStep</c> that parse all method's info into <c>RefactorMethod</c> objects
    ///     and save each method on it's respective <c>RefactorScope</c>.
    /// </summary>
    public class ParseMethodsStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null || state.RootScope == null) throw new ArgumentNullException();

            var parser = new MethodParser(state.Program, state.RootScope);
            parser.Parse();

            base.Handle(state);
        }
    }

    internal class MethodParser : DafnyVisitorWithNearests
    {
        protected Program program;
        protected IInlineScope rootScope;

        public MethodParser(Program program, IInlineScope rootScope)
        {
            if (program == null || rootScope == null) throw new ArgumentNullException();

            this.program = program;
            this.rootScope = rootScope;
        }

        protected IInlineScope CurScope => rootScope.FindInlineScope(nearestScopeToken?.GetHashCode() ?? 0);

        public void Parse()
        {
            Visit(program);
        }

        protected override void Visit(Method mt)
        {
            CurScope.InsertMethod(mt);
            var refactorMethod = CurScope.LookupMethod(mt.GetHashCode());

            foreach (var @in in mt.Ins)
            {
                var canBeModified = false;

                foreach (var frameExpression in mt.Mod.Expressions)
                {
                    if (frameExpression.E is NameSegment nameSeg && @in.Name == nameSeg.Name)
                    {
                        canBeModified = true;
                    }
                }

                refactorMethod.InsertArg(@in.Name, @in.Type, true, false, canBeModified);
            }

            base.Visit(mt);
        }
    }
}
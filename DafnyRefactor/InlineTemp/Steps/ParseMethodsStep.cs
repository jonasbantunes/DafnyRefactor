using System;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    public class ParseMethodsStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.Program == null || state.RootScope == null) throw new ArgumentNullException();

            var parser = new ParseMethodVisitor(state.Program, state.RootScope);
            parser.Parse();
            base.Handle(state);
        }
    }

    internal class ParseMethodVisitor : DafnyVisitorWithNearests
    {
        protected Program program;
        protected IInlineScope rootScope;

        public ParseMethodVisitor(Program program, IInlineScope rootScope)
        {
            if (program == null || rootScope == null) throw new ArgumentNullException();

            this.program = program;
            this.rootScope = rootScope;
        }

        public void Parse()
        {
            Visit(program);
        }

        protected override void Visit(Method mt)
        {
            var curScope = rootScope.FindInlineScope(nearestScopeToken?.GetHashCode() ?? 0);
            curScope.InsertMethod(mt);
            var refactorMethod = curScope.LookupMethod(mt.GetHashCode());
            if (refactorMethod != null)
            {
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
            }

            base.Visit(mt);
        }
    }
}
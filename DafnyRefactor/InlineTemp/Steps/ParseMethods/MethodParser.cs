using System;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp
{
    public class MethodParser : DafnyVisitorWithNearests
    {
        private readonly Program _program;
        private readonly IInlineScope _rootScope;

        private MethodParser(Program program, IInlineScope rootScope)
        {
            if (program == null || rootScope == null) throw new ArgumentNullException();

            _program = program;
            _rootScope = rootScope;
        }

        private IInlineScope CurScope => _rootScope.FindInlineScope(nearestScopeToken?.GetHashCode() ?? 0);

        private void Execute()
        {
            Visit(_program);
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

        public static void Parse(Program program, IInlineScope rootScope)
        {
            var parser = new MethodParser(program, rootScope);
            parser.Execute();
        }
    }
}
using System;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp
{
    public class VariableLocator : DafnyVisitorWithNearests
    {
        private readonly int _position;
        private readonly Program _program;
        private readonly IInlineScope _rootScope;
        private IInlineVariable _foundDeclaration;

        private VariableLocator(Program program, IInlineScope rootScope, int position)
        {
            if (program == null || rootScope == null) throw new ArgumentNullException();

            _program = program;
            _rootScope = rootScope;
            _position = position;
        }

        private IInlineScope CurTable => _rootScope.FindInlineScope(nearestScopeToken.GetHashCode());

        private void Execute()
        {
            Visit(_program);
        }

        protected override void Visit(VarDeclStmt vds)
        {
            foreach (var local in vds.Locals)
            {
                var start = local.Tok.pos;
                var end = local.EndTok.pos;
                if (start > _position || _position > end) continue;

                _foundDeclaration = CurTable.LookupInlineVariable(local.Name);
            }

            base.Visit(vds);
        }

        protected override void Visit(NameSegment nameSeg)
        {
            var start = nameSeg.tok.pos;
            var end = nameSeg.tok.pos + nameSeg.tok.val.Length;
            if (start > _position || _position > end) return;

            _foundDeclaration = CurTable.LookupInlineVariable(nameSeg.Name);
        }

        protected override void Visit(IdentifierExpr identifierExpr)
        {
            var start = identifierExpr.tok.pos;
            var end = identifierExpr.tok.pos + identifierExpr.tok.val.Length;
            if (start > _position || _position > end) return;

            _foundDeclaration = CurTable.LookupInlineVariable(identifierExpr.Name);

            base.Visit(identifierExpr);
        }

        protected override void Visit(ExprDotName exprDotName)
        {
        }

        protected override void Visit(MemberSelectExpr memmMemberSelectExpr)
        {
        }

        public static IInlineVariable Locate(Program program, IInlineScope rootScope, int position)
        {
            var locator = new VariableLocator(program, rootScope, position);
            locator.Execute();
            return locator._foundDeclaration;
        }
    }
}
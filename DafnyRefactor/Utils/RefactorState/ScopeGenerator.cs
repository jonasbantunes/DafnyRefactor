using System;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils
{
    /// <summary>
    ///     ParseVariables a <c>Dafny.Program</c> and generate it's respectives <c>IRefactorScope</c>.
    /// </summary>
    public class ScopeGenerator<TScope> : DafnyVisitorWithNearests where TScope : IRefactorScope, new()
    {
        private readonly Program _program;
        private TScope _generatedScope;

        private ScopeGenerator(Program program)
        {
            _program = program ?? throw new ArgumentNullException();
        }

        private void Execute()
        {
            _generatedScope = new TScope();
            Visit(_program);
        }

        protected override void Visit(ClassDecl cd)
        {
            var curTable = _generatedScope.FindScope(nearestScopeToken?.GetHashCode() ?? 0);
            curTable.InsertScope(cd.tok);

            base.Visit(cd);
        }

        protected override void Visit(VarDeclStmt vds)
        {
            foreach (var local in vds.Locals)
            {
                var curTable = _generatedScope.FindScope(nearestScopeToken?.GetHashCode() ?? 0);
                curTable.InsertVariable(local, vds);
            }
        }

        protected override void Visit(BlockStmt block)
        {
            var curTable = _generatedScope.FindScope(nearestScopeToken?.GetHashCode() ?? 0);
            curTable.InsertScope(block.Tok);

            base.Visit(block);
        }

        public static TScope Generate(Program program)
        {
            var generator = new ScopeGenerator<TScope>(program);
            generator.Execute();
            return generator._generatedScope;
        }
    }
}
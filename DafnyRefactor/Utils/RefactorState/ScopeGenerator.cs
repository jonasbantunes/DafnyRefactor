using System;
using Microsoft.Dafny;

namespace DafnyRefactor.Utils
{
    /// <summary>
    ///     ParseVariables a <c>Dafny.Program</c> and generate it's respectives <c>IRefactorScope</c>.
    /// </summary>
    public class ScopeGenerator<TScope> : DafnyVisitorWithNearests where TScope : IRefactorScope, new()
    {
        protected TScope generatedScope;
        protected Program program;

        protected ScopeGenerator(Program program)
        {
            this.program = program ?? throw new ArgumentNullException();
        }

        protected virtual void Execute()
        {
            generatedScope = new TScope();
            Visit(program);
        }

        protected override void Visit(ClassDecl cd)
        {
            var curTable = generatedScope.FindScope(nearestScopeToken?.GetHashCode() ?? 0);
            curTable.InsertScope(cd.tok);

            base.Visit(cd);
        }

        protected override void Visit(VarDeclStmt vds)
        {
            foreach (var local in vds.Locals)
            {
                var curTable = generatedScope.FindScope(nearestScopeToken?.GetHashCode() ?? 0);
                curTable.InsertVariable(local, vds);
            }
        }

        protected override void Visit(BlockStmt block)
        {
            var curTable = generatedScope.FindScope(nearestScopeToken?.GetHashCode() ?? 0);
            curTable.InsertScope(block.Tok);

            base.Visit(block);
        }

        public static TScope Generate(Program program)
        {
            var generator = new ScopeGenerator<TScope>(program);
            generator.Execute();
            return generator.generatedScope;
        }
    }
}
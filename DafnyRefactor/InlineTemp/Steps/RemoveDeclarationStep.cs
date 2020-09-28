using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dafny;
using Microsoft.DafnyRefactor.Utils;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    public class RemoveDeclarationStep<TState> : RefactorStep<TState> where TState : IInlineState
    {
        public override void Handle(TState state)
        {
            if (state == null || state.InlineVariable == null || state.Program == null || state.RootScope == null)
                throw new ArgumentNullException();

            var remover = new RemoveDeclarationVisitor(state.Program, state.RootScope, state.InlineVariable);
            remover.Execute();
            state.SourceEdits.AddRange(remover.Edits);

            base.Handle(state);
        }
    }

    internal class RemoveDeclarationVisitor : DafnyVisitorWithNearests
    {
        protected IInlineVariable inlineVar;
        protected Program program;
        protected IRefactorScope rootTable;

        public RemoveDeclarationVisitor(Program program, IRefactorScope rootTable, IInlineVariable inlineVar)
        {
            if (program == null || rootTable == null || inlineVar == null) throw new ArgumentNullException();

            this.program = program;
            this.inlineVar = inlineVar;
            this.rootTable = rootTable;
        }

        protected IRefactorScope CurScope => rootTable.FindScope(nearestScopeToken.GetHashCode());
        public List<SourceEdit> Edits { get; protected set; }

        public virtual void Execute()
        {
            Edits = new List<SourceEdit>();
            Visit(program);
        }

        protected override void Visit(VarDeclStmt vds)
        {
            if (vds.Locals.Count == 0) return;
            if (vds.Locals.Count == 1)
            {
                RemoveSingleVds(vds);
                return;
            }

            if (vds.Update == null)
            {
                ChangeVdsWithoutUp(vds);
                return;
            }

            ChangeVds(vds);
        }

        protected void RemoveSingleVds(VarDeclStmt vds)
        {
            var localName = vds.Locals[0].Name;
            var variable = CurScope.LookupVariable(localName);
            if (!variable.Equals(inlineVar)) return;

            var startPos = vds.Tok.pos;
            var endPos = vds.EndTok.pos + 1;
            Edits.Add(new SourceEdit(startPos, endPos, ""));
        }

        protected void ChangeVdsWithoutUp(VarDeclStmt vds)
        {
            int index;
            for (index = vds.Locals.Count - 1; index >= 0; index--)
            {
                var local = vds.Locals[index];
                var variable = CurScope.LookupVariable(local.Name);
                if (!variable.Equals(inlineVar)) continue;

                break;
            }

            if (index < 0) return;

            var newVds = new VarDeclStmt(vds.Tok, vds.EndTok, vds.Locals.ToList(), null);
            newVds.Locals.RemoveAt(index);

            var startPos = vds.Tok.pos;
            var endPos = vds.EndTok.pos + 1;
            var stringStmt = Printer.StatementToString(newVds);
            Edits.Add(new SourceEdit(startPos, endPos, stringStmt));
        }

        protected void ChangeVds(VarDeclStmt vds)
        {
            if (!(vds.Update is UpdateStmt up)) return;

            int index;
            for (index = up.Lhss.Count - 1; index >= 0; index--)
            {
                if (!(up.Lhss[index] is AutoGhostIdentifierExpr agie)) continue;
                var variable = CurScope.LookupVariable(agie.Name);
                if (!variable.Equals(inlineVar)) continue;

                break;
            }

            if (index < 0) return;

            var newUpdate = new UpdateStmt(up.Tok, up.EndTok, up.Lhss.ToList(), up.Rhss.ToList());
            var newVds = new VarDeclStmt(vds.Tok, vds.EndTok, vds.Locals.ToList(), newUpdate);
            newUpdate.Lhss.RemoveAt(index);
            newUpdate.Rhss.RemoveAt(index);
            newVds.Locals.RemoveAt(index);

            var startPos = vds.Tok.pos;
            var endPos = vds.EndTok.pos + 1;
            var stringStmt = Printer.StatementToString(newVds);
            var edit = new SourceEdit(startPos, endPos, stringStmt);
            Edits.Add(edit);
        }

        protected override void Visit(UpdateStmt up)
        {
            if (up.Lhss.Count == 1)
            {
                RemoveSingleUp(up);
            }
            else if (up.Lhss.Count > 1)
            {
                ChangeMultiUp(up);
            }
        }

        protected void RemoveSingleUp(UpdateStmt up)
        {
            if (!(up.Lhss[0] is NameSegment upNm)) return;
            var variable = CurScope.LookupVariable(upNm.Name);
            if (variable == null || !variable.Equals(inlineVar)) return;

            var startPos = up.Tok.pos;
            var endPos = up.EndTok.pos + 1;
            Edits.Add(new SourceEdit(startPos, endPos, ""));
        }

        protected void ChangeMultiUp(UpdateStmt up)
        {
            int index;
            for (index = up.Lhss.Count - 1; index >= 0; index--)
            {
                if (!(up.Lhss[index] is NameSegment nm)) continue;
                var variable = CurScope.LookupVariable(nm.Name);
                if (!variable.Equals(inlineVar)) continue;

                break;
            }

            if (index < 0) return;

            var newUpdate = new UpdateStmt(up.Tok, up.EndTok, up.Lhss.ToList(), up.Rhss.ToList());
            newUpdate.Lhss.RemoveAt(index);
            newUpdate.Rhss.RemoveAt(index);

            var startPos = up.Lhss[0].tok.pos;
            var endPos = up.EndTok.pos + 1;
            var stringStmt = Printer.StatementToString(newUpdate);
            Edits.Add(new SourceEdit(startPos, endPos, stringStmt));
        }
    }
}
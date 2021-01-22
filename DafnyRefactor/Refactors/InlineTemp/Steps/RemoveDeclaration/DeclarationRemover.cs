using System;
using System.Collections.Generic;
using System.Linq;
using DafnyRefactor.Utils;
using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp
{
    public class DeclarationRemover : DafnyVisitorWithNearests
    {
        private readonly IInlineVariable _inlineVar;
        private readonly Program _program;
        private readonly IRefactorScope _rootTable;
        private List<SourceEdit> _edits;

        private DeclarationRemover(Program program, IRefactorScope rootTable, IInlineVariable inlineVar)
        {
            if (program == null || rootTable == null || inlineVar == null) throw new ArgumentNullException();

            _program = program;
            _inlineVar = inlineVar;
            _rootTable = rootTable;
        }

        private IRefactorScope CurScope => _rootTable.FindScope(nearestScopeToken.GetHashCode());

        private void Execute()
        {
            _edits = new List<SourceEdit>();
            Visit(_program);
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

        private void RemoveSingleVds(VarDeclStmt vds)
        {
            var localName = vds.Locals[0].Name;
            var variable = CurScope.LookupVariable(localName);
            if (!variable.Equals(_inlineVar)) return;

            var startPos = vds.Tok.pos;
            var endPos = vds.EndTok.pos + 1;
            _edits.Add(new SourceEdit(startPos, endPos, ""));
        }

        private void ChangeVdsWithoutUp(VarDeclStmt vds)
        {
            int index;
            for (index = vds.Locals.Count - 1; index >= 0; index--)
            {
                var local = vds.Locals[index];
                var variable = CurScope.LookupVariable(local.Name);
                if (!variable.Equals(_inlineVar)) continue;

                break;
            }

            if (index < 0) return;

            var newVds = new VarDeclStmt(vds.Tok, vds.EndTok, vds.Locals.ToList(), null);
            newVds.Locals.RemoveAt(index);

            var startPos = vds.Tok.pos;
            var endPos = vds.EndTok.pos + 1;
            var stringStmt = Printer.StatementToString(newVds);
            _edits.Add(new SourceEdit(startPos, endPos, stringStmt));
        }

        private void ChangeVds(VarDeclStmt vds)
        {
            if (!(vds.Update is UpdateStmt up)) return;

            int index;
            for (index = up.Lhss.Count - 1; index >= 0; index--)
            {
                if (!(up.Lhss[index] is AutoGhostIdentifierExpr agie)) continue;
                var variable = CurScope.LookupVariable(agie.Name);
                if (!variable.Equals(_inlineVar)) continue;

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
            _edits.Add(edit);
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

        private void RemoveSingleUp(UpdateStmt up)
        {
            if (!(up.Lhss[0] is NameSegment upNm)) return;
            var variable = CurScope.LookupVariable(upNm.Name);
            if (variable == null || !variable.Equals(_inlineVar)) return;

            var startPos = up.Tok.pos;
            var endPos = up.EndTok.pos + 1;
            _edits.Add(new SourceEdit(startPos, endPos, ""));
        }

        private void ChangeMultiUp(UpdateStmt up)
        {
            int index;
            for (index = up.Lhss.Count - 1; index >= 0; index--)
            {
                if (!(up.Lhss[index] is NameSegment nm)) continue;
                var variable = CurScope.LookupVariable(nm.Name);
                if (!variable.Equals(_inlineVar)) continue;

                break;
            }

            if (index < 0) return;

            var newUpdate = new UpdateStmt(up.Tok, up.EndTok, up.Lhss.ToList(), up.Rhss.ToList());
            newUpdate.Lhss.RemoveAt(index);
            newUpdate.Rhss.RemoveAt(index);

            var startPos = up.Lhss[0].tok.pos;
            var endPos = up.EndTok.pos + 1;
            var stringStmt = Printer.StatementToString(newUpdate);
            _edits.Add(new SourceEdit(startPos, endPos, stringStmt));
        }

        public static List<SourceEdit> Remove(Program program, IRefactorScope rootTable, IInlineVariable inlineVar)
        {
            var remover = new DeclarationRemover(program, rootTable, inlineVar);
            remover.Execute();
            return remover._edits;
        }
    }
}
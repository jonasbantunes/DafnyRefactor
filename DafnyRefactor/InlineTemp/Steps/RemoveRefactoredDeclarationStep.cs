using System.Collections.Generic;

namespace Microsoft.Dafny
{
    public class RemoveRefactoredDeclarationStep : DafnyWithTableVisitor
    {
        protected SymbolTableDeclaration declaration;
        public List<SourceEdit> Edits { get; protected set; }

        public RemoveRefactoredDeclarationStep(Program program, SymbolTable rootTable, SymbolTableDeclaration declaration) : base(program, rootTable)
        {
            this.declaration = declaration;
        }

        public override void Execute()
        {
            Edits = new List<SourceEdit>();
            base.Execute();
        }

        protected override VarDeclStmt Visit(VarDeclStmt vds)
        {
            if (vds.Locals.Count == 1 && vds.Update?.Lhss?.Count == 1 && curTable.LookupDeclaration(vds.Locals[0].Name).GetHashCode() == declaration.GetHashCode())
            {
                Edits.Add(new SourceEdit(vds.Tok.pos, vds.EndTok.pos + 1, ""));
            }

            return base.Visit(vds);
        }
    }
}

using System.Collections.Generic;

namespace Microsoft.Dafny
{
    public class RemoveRefactoredDeclarationStep : DafnyWithTableVisitor
    {
        protected SymbolTableDeclaration declaration;

        public RemoveRefactoredDeclarationStep(Program program, SymbolTable rootTable, SymbolTableDeclaration declaration) : base(program, rootTable)
        {
            this.declaration = declaration;
        }

        protected override List<Statement> Traverse(List<Statement> body)
        {
            for (int i = body.Count - 1; i >= 0; i--)
            {
                Statement st = body[i];
                body[i] = Visit(st);
                if (body[i] == null)
                {
                    body.RemoveAt(i);
                }
            }

            return body;
        }

        protected override VarDeclStmt Visit(VarDeclStmt vds)
        {
            if (vds.Locals.Count == 1 && vds.Update?.Lhss?.Count == 1 && curTable.Lookup(vds.Locals[0].Name).GetHashCode() == declaration.GetHashCode())
            {
                return null;
            }

            return vds;
        }
    }
}

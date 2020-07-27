namespace Microsoft.Dafny
{
    public class RemoveRefactoredDeclarationStep : DafnyWithTableVisitor
    {
        protected SymbolTableDeclaration declaration;

        public RemoveRefactoredDeclarationStep(Program program, SymbolTable rootTable, SymbolTableDeclaration declaration) : base(program, rootTable)
        {
            this.declaration = declaration;
        }

        protected override Method Visit(Method mt)
        {
            for (int i = mt.Body.Body.Count - 1; i >= 0; i--)
            {
                Statement st = mt.Body.Body[i];
                mt.Body.Body[i] = Visit(st);
                if (mt.Body.Body[i] == null)
                {
                    mt.Body.Body.RemoveAt(i);
                }
            }

            return mt;
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

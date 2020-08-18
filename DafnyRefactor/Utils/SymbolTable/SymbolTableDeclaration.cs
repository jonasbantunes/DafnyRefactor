using Microsoft.Boogie;

namespace Microsoft.Dafny
{
    public class SymbolTableDeclaration
    {
        public int hashCode;
        public string name;
        public IToken tok;

        public override int GetHashCode()
        {
            return hashCode;
        }
    }
}

namespace Microsoft.Dafny
{
    public class InlineRefactor
    {
        public Program program;
        public string method;
        public string name;

        public InlineRefactor(Program program, string method, string name)
        {
            this.program = program;
            this.method = method;
            this.name = name;
        }

        public void refactor()
        {
            var inlineRetriever = new InlineRetrieveStep(program, method, name);
            inlineRetriever.execute();
            var inVar = inlineRetriever.inlineVar;

            if (inVar.isUpdated)
            {
                return;
            }

            var refactor = new InlineRefactorStep(program, inVar);
            refactor.execute();
        }
    }
}

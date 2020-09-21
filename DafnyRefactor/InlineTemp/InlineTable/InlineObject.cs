using Microsoft.Dafny;

namespace Microsoft.DafnyRefactor.InlineTemp
{
    public interface IInlineObject
    {
        string Name { get; set; }
        Type Type { get; set; }
    }

    public class InlineObject : IInlineObject
    {
        public InlineObject(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; set; }
        public Type Type { get; set; }
    }
}
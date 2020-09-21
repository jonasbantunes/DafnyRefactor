using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp.InlineTable
{
    public interface IInlineObject
    {
        string Name { get; set; }
        Type Type { get; set; }
    }

    public class InlineObject : IInlineObject
    {
        public string Name { get; set; }
        public Type Type { get; set; }

        public InlineObject(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }
}
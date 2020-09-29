using Microsoft.Dafny;

namespace DafnyRefactor.InlineTemp.State
{
    /// <summary>
    ///     Represents a <c>RefactorMethod</c> argument info.
    /// </summary>
    public interface IMethodArg
    {
        string Name { get; set; }
        Type Type { get; set; }
        bool IsInput { get; set; }
        bool IsOutput { get; set; }
        bool CanBeModified { get; set; }
    }

    public class MethodArg : IMethodArg
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public bool IsInput { get; set; }
        public bool IsOutput { get; set; }
        public bool CanBeModified { get; set; }
    }
}
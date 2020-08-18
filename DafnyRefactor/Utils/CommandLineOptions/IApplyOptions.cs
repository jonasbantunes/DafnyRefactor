namespace Microsoft.Dafny
{
    public interface IApplyOptions
    {
        string FilePath { get; set; }
        bool Stdout { get; set; }
    }
}

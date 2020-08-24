namespace DafnyRefactor.Utils.CommandLineOptions
{
    public interface IApplyOptions
    {
        string FilePath { get; set; }
        bool Stdout { get; set; }
    }
}
using System.Text;

namespace BigFileSort.Domain;

public interface IFileSorter
{
    void SortFile(SortFileCommand command);
}

public record SortFileCommand
{
    public string InputFileName { get; init; }
    public string OutputFileName { get; init; }
    public int MemoryLimitInBytes { get; init; }
    
    public byte[] Delimiter { get; init; } = { (byte)'\r', (byte)'\n'};
    
    public IFileParser? FileParser { get; init; }
    public IFileMerger FileMerger { get; init; }
    
    public Encoding? Encoding { get; init; }
}
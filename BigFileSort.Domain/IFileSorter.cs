using System.Text;

namespace BigFileSort.Domain;

/// <summary>
/// File sorter.
/// </summary>
public interface IFileSorter
{
    /// <summary>
    /// Sorts file.
    /// </summary>
    /// <param name="command">Sort parameters.</param>
    ParseMetrics SortFile(SortFileCommand command);
}

public record SortFileCommand
{
    public BigFileSortConfiguration Configuration { get; init; }
    
    public string InputFileName { get; init; }
    public string OutputFileName { get; init; }
    public int MemoryLimitInBytes { get; init; }
    public byte[] Delimiter { get; init; }
    public IFileParser? FileParser { get; init; }
    public IFileMerger FileMerger { get; init; }
    public Encoding? Encoding { get; init; }
}
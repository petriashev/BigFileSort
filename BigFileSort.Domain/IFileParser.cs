namespace BigFileSort.Domain;

/// <summary>
/// Represents file parser.
/// </summary>
public interface IFileParser
{
    /// <summary>
    /// Parses input buffer into provided dictionary.
    /// </summary>
    ParseResult ReadAndParse(ParseContext parseContext);
}

public record ParseContext
{
    public SortFileCommand Command { get; init; }
    
    public List<FileName> Files { get; init; }
    
    public Dictionary<string, List<int>> TargetIndex { get; init; } = new();

    public Dictionary<VirtualString, List<int>> VirtualTargetIndex { get; init; } = new();
    
    public int Iteration { get; init; }
    
    public MemoryBuffer Buffer { get; init; }
}

public record ParseResult(int TotalLines);
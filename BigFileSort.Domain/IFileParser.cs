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
    
    public byte[] Buffer { get; init; }
    
    public int BufferLength { get; init; }

    public MemoryStream SourceBuffer => new MemoryStream(Buffer, 0, BufferLength);
    
    public Dictionary<string, List<int>> TargetIndex { get; init; }
    
    public List<FileName> Files { get; init; }
    
    public int Iteration { get; init; }
}

public record ParseResult(int TotalLines);
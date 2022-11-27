using System.Collections.Concurrent;
using BigFileSort.Sorting;
using BigFileSort.System;

namespace BigFileSort.Parsing;

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

    public ConcurrentDictionary<VirtualString, List<int>> VirtualTargetIndex { get; init; } = new();
    
    public int Iteration { get; init; }
    
    public MemoryBuffer Buffer { get; init; }

    public ParseMetrics Metrics { get; init; } = new ParseMetrics();
}

public class ParseMetrics
{
    private int _totalLines;
    private long _totalBytes;
    private int _maxNumbersPerString;

    public int TotalLines=> _totalLines;
    
    public long TotalBytes => _totalBytes;

    public int MaxNumbersPerString => _maxNumbersPerString;

    public ParseMetrics(int totalLines = 0, long totalBytes = 0, int maxNumbersPerString = 64)
    {
        _totalLines = totalLines;
        _totalBytes = totalBytes;
        _maxNumbersPerString = maxNumbersPerString;
    }

    public void Increment(int lines, long bytes)
    {
        Interlocked.Add(ref _totalLines, lines);
        Interlocked.Add(ref _totalBytes, bytes);
    }

    public void SetNumbersCount(int numbersCount)
    {
        if (numbersCount > _maxNumbersPerString)
            _maxNumbersPerString = numbersCount;
    }
}

public record ParseResult(int TotalLines);
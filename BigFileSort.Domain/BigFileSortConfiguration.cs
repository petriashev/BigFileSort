using BigFileSort.System;

namespace BigFileSort;

/// <summary>
/// BigFile start configuration.
/// </summary>
public record BigFileSortConfiguration
{
    /// <summary>
    /// Gets or sets command to run.
    /// </summary>
    public FileCommand Command { get; init; }
    
    /// <summary>
    /// Gets or sets the working directory for file operations.
    /// </summary>
    public string? WorkingDirectory { get; init; }
    
    /// <summary>
    /// Gets or sets Generate file options.
    /// </summary>
    public GenerateFileConfiguration Generate { get; init; }
    
    /// <summary>
    /// Gets or sets Sort file options.
    /// </summary>
    public SortFileConfiguration Sort { get; init; }
}

public enum FileCommand
{
    Generate,
    Sort
}

public class GenerateFileConfiguration
{
    public string OutputFileName { get; init; } = "generated.txt";
    public int? SizeInMegabytes { get; init; }
    public int? SizeInGigabytes { get; init; }
}

public class SortFileConfiguration
{
    public string InputFileName { get; init; } = "generated.txt";
    
    public string OutputFileName { get; init; } = "sorted_{0}.txt";

    public int BufferSize { get; init; } = 1025;
    
    public DataUnit BufferSizeUnit { get; init; } = DataUnit.MEGABYTES;
    
    public byte[] Delimiter{ get; init; } = {13, 10};
    
    public string FileParser { get; init; } = "BufferFileParser";
    
    /// <summary>
    /// MergeIterative=true: Мержит сортированные промежуточные файлы попарно, пока не останется только один.
    /// MergeIterative=false: Мержит сортированные промежуточные файлы одновременно.
    /// </summary>
    public bool MergeIterative { get; init; } = false;
    
    /// <summary>
    /// Удалять промежуточные файлы после мержа.
    /// </summary>
    public bool DeleteTempFiles { get; init; } = false;
    
    /// <summary>
    /// Использовать многопоточный парсинг и индексирование.
    /// </summary>
    public bool UseMultithreading { get; init; } = false;
    
    /// <summary>
    /// Количество потоков при использовании UseMultithreading. 
    /// </summary>
    public int Threads { get; init; } = 2;
}

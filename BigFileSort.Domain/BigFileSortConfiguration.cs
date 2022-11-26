namespace BigFileSort;

public record BigFileSortConfiguration
{
    public FileCommand Command { get; init; }
    public string? WorkingDirectory { get; init; }
    
    public GenerateFileConfiguration Generate { get; init; }
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

    public int? MemoryLimitInMegabytes { get; init; } = 1025;
    
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

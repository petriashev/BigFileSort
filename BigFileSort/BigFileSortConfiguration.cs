namespace BigFileSort;

public class BigFileSortConfiguration
{
    public FileCommand Command { get; set; }
    public string? WorkingDirectory { get; set; }
    
    public GenerateFileConfiguration Generate { get; set; }
    public SortFileConfiguration Sort { get; set; }
}

public enum FileCommand
{
    Generate,
    Sort
}

public class GenerateFileConfiguration
{
    public string OutputFileName { get; set; } = "generated.txt";
    public int? SizeInMegabytes { get; set; }
    public int? SizeInGigabytes { get; set; }
}

public class SortFileConfiguration
{
    public string InputFileName { get; set; } = "generated.txt";
    
    public string OutputFileName { get; set; } = "sorted_{0}.txt";

    public int? MemoryLimitInMegabytes { get; set; } = 1025;
    
    public byte[] Delimiter{ get; set; } = {13, 10};
    
    public string FileParser { get; set; } = "BufferFileParser";
}


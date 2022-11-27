using System.Diagnostics;
using BigFileSort.Generation;
using BigFileSort.Parsing;
using BigFileSort.Sorting;
using BigFileSort.System;

namespace BigFileSort;

public class BigFileSortApp
{
    private readonly BigFileSortConfiguration _configuration;

    public BigFileSortApp(BigFileSortConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void GenerateFile()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        
        var settings = _configuration.Generate;
        var outputFileName = settings.OutputFileName;
        var inBytes = settings.SizeInGigabytes?.GigabytesInBytes() ??
                      settings.SizeInMegabytes?.MegabytesInBytes() ?? 1.GigabytesInBytes();

        Console.WriteLine($"OutputFileName: {outputFileName}");
        Console.WriteLine($"TargetSizeInBytes: {inBytes}");

        using var fileStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write);
        fileStream.Position = 0;

        IFileGenerator fileGenerator = new FileGenerator();
        fileGenerator.GenerateFile(fileStream, new GenerateFileCommand { TargetFileSize = inBytes });
        Console.WriteLine($"GeneratedSizeInBytes: {new FileInfo(outputFileName).Length}");

        Console.WriteLine($"Generated in {stopwatch.Elapsed}");
    }

    public void Sort()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        
        var MAX_BYTE_ARRAY_SIZE = 0X7FEFFFFF;

        var sort = _configuration.Sort;
        var sortFileCommand = new SortFileCommand()
        {
            Configuration = _configuration,
            InputFileName = sort.InputFileName,
            OutputFileName = sort.OutputFileName,
        
            BuggerSizeInBytes = (int)sort.BufferSize.GetDataSizeInBytes(sort.BufferSizeUnit),
            Delimiter = sort.Delimiter,
        
            // BufferFileParser на данный момент самый быстрый
            FileParser = sort.FileParser == "BufferFileParser"? new BufferFileParser()
                : sort.FileParser == "StreamReaderParser" ? new StreamReaderParser()
                : new BufferFileParser(),
    
            FileMerger = new StreamFileMerger(_configuration)
        };

        if (sortFileCommand.BuggerSizeInBytes <= 0 || sortFileCommand.BuggerSizeInBytes > MAX_BYTE_ARRAY_SIZE)
            sortFileCommand = sortFileCommand with { BuggerSizeInBytes = MAX_BYTE_ARRAY_SIZE };
    
        //sortFileCommand = sortFileCommand with { InputFileName = "sample.txt", MemoryLimitInBytes = 60 };
        //sortFileCommand = sortFileCommand with { InputFileName = "generated.txt", MemoryLimitInBytes = 520.MegabytesInBytes() };
        //sortFileCommand = sortFileCommand with { InputFileName = "generated.txt", MemoryLimitInBytes = 1025.MegabytesInBytes() };
        //sortFileCommand = sortFileCommand with { InputFileName = "generated10.txt", MemoryLimitInBytes = MAX_BYTE_ARRAY_SIZE };

        Console.WriteLine($"InputFileName: {sortFileCommand.InputFileName}");
        Console.WriteLine($"FileParser: {sortFileCommand.FileParser.GetType().Name}");
        Console.WriteLine($"MemoryLimitInBytes: {sortFileCommand.BuggerSizeInBytes}");
    
        IFileSorter fileSorter = new FileSorter();
        var metrics = fileSorter.SortFile(sortFileCommand);

        Console.WriteLine($"Sorted in {stopwatch.Elapsed}");
    }
}
using System.Diagnostics;
using BigFileSort;
using BigFileSort.Domain;
using Newtonsoft.Json;

Console.WriteLine("BigFileSort");

var configurationText = File.ReadAllText("appsettings.json");
var configuration = JsonConvert.DeserializeObject<BigFileSortConfiguration>(configurationText);

if (configuration.WorkingDirectory != null)
{
    configuration = configuration with { WorkingDirectory = Path.GetFullPath(configuration.WorkingDirectory) };
    Directory.CreateDirectory(configuration.WorkingDirectory);
    Directory.SetCurrentDirectory(configuration.WorkingDirectory);
    Console.WriteLine($"WorkingDirectory: {configuration.WorkingDirectory}");
}

Console.WriteLine($"Command: {configuration.Command}");

Stopwatch stopwatch = Stopwatch.StartNew();

if (configuration.Command == FileCommand.Generate)
{
    var settings = configuration.Generate;
    var outputFileName = settings.OutputFileName;
    var inBytes = settings.SizeInGigabytes?.GigabytesInBytes() ?? settings.SizeInMegabytes?.MegabytesInBytes() ?? 1.GigabytesInBytes();
    
    Console.WriteLine($"OutputFileName: {outputFileName}");
    Console.WriteLine($"TargetSizeInBytes: {inBytes}");
    
    using var fileStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write);
    fileStream.Position = 0;
    
    IFileGenerator fileGenerator = new FileGenerator();
    fileGenerator.GenerateFile(fileStream, new GenerateFileCommand { TargetFileSize = inBytes});
    Console.WriteLine($"GeneratedSizeInBytes: {new FileInfo(outputFileName).Length}");
    
    Console.WriteLine($"Generated in {stopwatch.Elapsed}");
    return;
}

if (configuration.Command == FileCommand.Sort)
{
    var MAX_BYTE_ARRAY_SIZE = 0X7FEFFFFF;
    
    var fileSorter = new FileSorter();
    var sortFileCommand = new SortFileCommand()
    {
        Configuration = configuration,
        InputFileName = configuration.Sort.InputFileName,
        OutputFileName = configuration.Sort.OutputFileName,
        
        // Одним куском минимальное время, но максимальная память
        MemoryLimitInBytes = configuration.Sort.MemoryLimitInMegabytes?.MegabytesInBytes() ?? MAX_BYTE_ARRAY_SIZE,
        Delimiter = configuration.Sort.Delimiter,
        
        // BufferFileParser на данный момент самый быстрый
        FileParser = configuration.Sort.FileParser == "BufferFileParser"? new BufferFileParser()
            : configuration.Sort.FileParser == "StreamReaderParser" ? new StreamReaderParser()
            : new BufferFileParser(),
    
        FileMerger = new StreamFileMerger(configuration)
    };

    if (sortFileCommand.MemoryLimitInBytes < 0 || sortFileCommand.MemoryLimitInBytes > MAX_BYTE_ARRAY_SIZE)
        sortFileCommand = sortFileCommand with{ MemoryLimitInBytes = MAX_BYTE_ARRAY_SIZE };
    
    //sortFileCommand = sortFileCommand with { InputFileName = "sample.txt", MemoryLimitInBytes = 60 };
    //sortFileCommand = sortFileCommand with { InputFileName = "generated.txt", MemoryLimitInBytes = 520.MegabytesInBytes() };
    //sortFileCommand = sortFileCommand with { InputFileName = "generated.txt", MemoryLimitInBytes = 1025.MegabytesInBytes() };
    //sortFileCommand = sortFileCommand with { InputFileName = "generated10.txt", MemoryLimitInBytes = MAX_BYTE_ARRAY_SIZE };

    Console.WriteLine($"InputFileName: {sortFileCommand.InputFileName}");
    Console.WriteLine($"MemoryLimitInBytes: {sortFileCommand.MemoryLimitInBytes}");
    
    fileSorter.SortFile(sortFileCommand);

    Console.WriteLine($"Sorted in {stopwatch.Elapsed}");
}

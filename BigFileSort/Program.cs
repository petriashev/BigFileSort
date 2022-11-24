using System.Diagnostics;
using BigFileSort;
using BigFileSort.Domain;
using Newtonsoft.Json;

Console.WriteLine("BigFileSort");

var configurationText = File.ReadAllText("appsettings.json");
var configuration = JsonConvert.DeserializeObject<BigFileSortConfiguration>(configurationText);

if (configuration.WorkingDirectory != null)
{
    configuration.WorkingDirectory = Path.GetFullPath(configuration.WorkingDirectory);
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
    
    using var fileStream = new FileStream(outputFileName, FileMode.OpenOrCreate);
    fileStream.Position = 0;
    IFileGenerator fileGenerator = new FileGenerator();
    fileGenerator.GenerateFile(fileStream, new GenerateFileCommand { TargetFileSize = inBytes});
    
    Console.WriteLine($"Generated in {stopwatch.Elapsed}");
    return;
}

if (configuration.Command == FileCommand.Sort)
{
    var MAX_BYTE_ARRAY_SIZE = 0X7FEFFFFF;
    
    var fileSorter = new FileSorter();
    var settings = configuration.Sort;
    var sortFileCommand = new SortFileCommand()
    {
        InputFileName = settings.InputFileName,
        OutputFileName = settings.OutputFileName,
        
        // Одним куском минимальное время, но максимальная память
        MemoryLimitInBytes = settings.MemoryLimitInMegabytes?.MegabytesInBytes() ?? MAX_BYTE_ARRAY_SIZE,
        Delimiter = settings.Delimiter,
        
        // BufferFileParser на данный момент самый быстрый
        FileParser = settings.FileParser == "BufferFileParser"? new BufferFileParser()
            : settings.FileParser == "StreamReaderParser" ? new StreamReaderParser()
            : new BufferFileParser(),
    
        FileMerger = new StreamFileMerger()
    };

    if (sortFileCommand.MemoryLimitInBytes < 0 || sortFileCommand.MemoryLimitInBytes > MAX_BYTE_ARRAY_SIZE)
        sortFileCommand = sortFileCommand with{ MemoryLimitInBytes = MAX_BYTE_ARRAY_SIZE };
    
    //sortFileCommand = sortFileCommand with { InputFileName = "sample.txt", MemoryLimitInBytes = 60 };
    //sortFileCommand = sortFileCommand with { InputFileName = "generated.txt", MemoryLimitInBytes = 520.MegabytesInBytes() };
    //sortFileCommand = sortFileCommand with { InputFileName = "generated.txt", MemoryLimitInBytes = 1025.MegabytesInBytes() };
    //sortFileCommand = sortFileCommand with { InputFileName = "generated10.txt", MemoryLimitInBytes = MAX_BYTE_ARRAY_SIZE };

    fileSorter.SortFile(sortFileCommand);

    Console.WriteLine($"Sorted in {stopwatch.Elapsed}");
}

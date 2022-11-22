using System.Diagnostics;
using System.Text;
using BigFileSort.Domain;

Console.WriteLine("Hello, World!");
Stopwatch stopwatch = Stopwatch.StartNew();

bool generate = false;
var MAX_BYTE_ARRAY_SIZE = 0X7FEFFFFF;

if (generate)
{
    using var fileStream = new FileStream("generated10.txt", FileMode.OpenOrCreate);
    fileStream.Position = 0;
    IFileGenerator fileGenerator = new FileGenerator();
    fileGenerator.GenerateFile(fileStream, new GenerateFileCommand{ TargetFileSize = 10.GigabytesInBytes()-200});
    
    Console.WriteLine($"Generated in {stopwatch.Elapsed}");
    return;
}

var fileSorter = new FileSorter();
var sortFileCommand = new SortFileCommand()
{
    InputFileName = @"C:\Users\petri\Documents\Projects\BigFileSort\BigFileSort\bin\Release\net6.0\generated10.txt",
    OutputFileName = "sorted_{0}.txt",
    // Одним куском минимальное время, но максимальная память
    //MemoryLimitInBytes = 1200.MegabytesInBytes(),
    //MemoryLimitInBytes = 520.MegabytesInBytes(),
    MemoryLimitInBytes = 1025.MegabytesInBytes(),
    Delimiter =  new []{ (byte)'\r', (byte)'\n'},
    Encoding = Encoding.UTF8,
    // BufferFileParser на данный момент самый быстрый
    //FileParser = new StreamReaderParser(),
    //FileParser = new SpanFileParser(),
    //FileParser = new BufferFileParser(),
    FileParser = new StreamReaderParser(),
    
    FileMerger = new StreamFileMerger()
};

Directory.SetCurrentDirectory(@"C:\Users\petri\Documents\Projects\BigFileSort\BigFileSort\bin\Release\net6.0");

//sortFileCommand = sortFileCommand with { InputFileName = "sample.txt", MemoryLimitInBytes = 60 };
sortFileCommand = sortFileCommand with { InputFileName = "generated.txt", MemoryLimitInBytes = 520.MegabytesInBytes() };
//sortFileCommand = sortFileCommand with { InputFileName = "generated10.txt", MemoryLimitInBytes = MAX_BYTE_ARRAY_SIZE };

fileSorter.SortFile(sortFileCommand);

Console.WriteLine($"Sorted in {stopwatch.Elapsed}");
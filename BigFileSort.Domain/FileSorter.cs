using System.Runtime.CompilerServices;
using System.Text;

namespace BigFileSort.Domain;

public sealed class FileSorter : IFileSorter
{
    /// <inheritdoc />
    public void SortFile(SortFileCommand command)
    {
        using var sortFile = new Measure($"SortFile");
        
        var memoryLimitInBytes = command.MemoryLimitInBytes;
        
        long position = 0;
        int iteration = 1;
        bool isLast = false;

        byte[] inputBytes = new byte[memoryLimitInBytes];
        var files = new List<FileName>();
        
        while (true)
        {
            using var splitIteration = new Measure($"SplitIteration {iteration}");
            
            var inputFileStream = File.OpenRead(command.InputFileName);
            inputFileStream.Position = position;
            
            if (iteration > 1)
                inputBytes.Clear();

            //new StreamReader(inputFileStream).ReadBlock(Unsafe.As<>(inputBytes), 0, memoryLimitInBytes);
            int bytesRead = inputFileStream.Read(inputBytes, 0, memoryLimitInBytes);
            if (bytesRead < memoryLimitInBytes)
                isLast = true;
            
            if (bytesRead == 0)
                break;
            
            int countCharsAligned = inputBytes.CountCharsAligned(bytesRead);
            if (isLast || countCharsAligned == 0 && bytesRead > 0)
            {
                countCharsAligned = bytesRead;
            }

            if (countCharsAligned == 0)
                break;
            
            var parseContext = new ParseContext
            {
                Command = command,
                Buffer = inputBytes,
                BufferLength = countCharsAligned,
                TargetIndex = new Dictionary<string, List<int>>(),
                Files = files,
                Iteration = iteration
            };
            
            SplitIteration(parseContext);
            
            if (isLast)
                break;

            position += countCharsAligned;
            iteration++;
        }
        
        if (files.Count > 1)
        {
            Queue<FileName> fileQueue = new Queue<FileName>(files);

            using var _ = new Measure("MergeFiles");

            // TODO: Settings
            bool iterative = false;

            if (!iterative)
            {
                var outputFile = command.BuildOutputName(files);
                using var fileMerge = new Measure($"MergeOutput: {outputFile.Name}");
                command.FileMerger.MergeFiles(files.ToArray(), outputFile);
            }
            else
            {
                while (fileQueue.Count > 1)
                {
                    var filesCount = fileQueue.Count;
                    var numFilesToMerge = 2;
                    for (int mergeIter = 0; mergeIter < filesCount; mergeIter+=numFilesToMerge)
                    {
                        var fileName1 = fileQueue.Dequeue();
                        var fileName2 = fileQueue.Dequeue();
                        var fileName3 = command.BuildOutputName(fileName1, fileName2);
                    
                        using var fileMerge = new Measure($"MergeOutput: {fileName3.Name}");

                        command.FileMerger.MergeFiles(fileName1, fileName2, fileName3);

                        fileQueue.Enqueue(fileName3);
                    }
                }
            }
        }
        
        int end = 0;
    }

    private static void SplitIteration(ParseContext parseContext)
    {
        var command = parseContext.Command;
        var index = parseContext.TargetIndex;
        var fileParser = parseContext.Command.FileParser;
        var files = parseContext.Files;
        int iteration = parseContext.Iteration;
        
        using (var readAndParse = new Measure($"ReadAndParse {iteration}"))
        {
            var parseResult = fileParser.ReadAndParse(parseContext);
            readAndParse.AddInfo($"TotalLines = {parseResult.TotalLines}");
        }

        if (index.Count > 0)
        {
            KeyValuePair<string, List<int>>[] orderedByText;
            using (new Measure("Sort"))
            {
                orderedByText = index.OrderBy(pair => pair.Key).ToArray();
            }

            var fileName = new FileName(string.Format(command.OutputFileName, iteration), iteration.ToString());
            files.Add(fileName);
            using (new Measure($"WriteSorted {fileName.ShortFileName}"))
            {
                using var streamWriter = new StreamWriter(fileName.Name);

                foreach (var keyValuePair in orderedByText)
                {
                    var text = keyValuePair.Key;
                    var orderedNumbers = keyValuePair.Value.OrderBy(n => n);
                    foreach (var number in orderedNumbers)
                    {
                        streamWriter.Write(number);
                        streamWriter.Write(". ");
                        streamWriter.Write(text);
                        streamWriter.Write(Environment.NewLine);
                    }
                }

                streamWriter.Flush();
                var streamLength = streamWriter.BaseStream.Length;
            }
        }
    }
}

public record FileName(string Name, string Suffix)
{
    public string ShortFileName => Path.GetFileName(Name);
}

public static class ReadExtensions
{
    public static void Clear(this byte[] inputBytes)
    {
        for (var i = 0; i < inputBytes.Length; i++)
        {
            inputBytes[i] = 0;
        }
    }

    public static int CountCharsAligned(this byte[] inputBytes, int count)
    {
        for (var i1 = count - 1; i1 >= 0; i1--)
        {
            if ((char)inputBytes[i1] == '\n')
            {
                return i1 + 1;
            }
        }

        return 0;
    }
    
    public static FileName BuildOutputName(this SortFileCommand command, IEnumerable<FileName> fileNames)
    {
        StringBuilder builder = null;
        foreach (var fileName in fileNames)
        {
            if (builder == null)
            {
                builder = new StringBuilder(fileName.Suffix);
            }
            else
            {
                builder.Append("+");
                builder.Append(fileName.Suffix);
            }
        }

        var suffix = builder.ToString();
                
        return new FileName(string.Format(command.OutputFileName, suffix), suffix);
    }

    public static FileName BuildOutputName(this SortFileCommand command, params FileName[] fileNames)
    {
        return BuildOutputName(command, (IEnumerable<FileName>)fileNames);
    }
}
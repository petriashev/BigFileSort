using System.Text;

namespace BigFileSort.Domain;

public sealed class FileSorter : IFileSorter
{
    /// <inheritdoc />
    public void SortFile(SortFileCommand command)
    {
        using var sortFile = new Measure($"SortFile");
        
        var memoryLimitInBytes = command.MemoryLimitInBytes;
        
        long filePosition = 0;
        int iteration = 1;
        bool isLast = false;

        byte[] inputBytes = new byte[memoryLimitInBytes];
        var files = new List<FileName>();
        
        var parseContext = new ParseContext
        {
            Command = command,
            Files = files,
        };
        
        while (true)
        {
            using var splitIteration = new Measure($"SplitIteration {iteration}");
            
            var inputFileStream = File.OpenRead(command.InputFileName);
            inputFileStream.Position = filePosition;
            
            if (iteration > 1)
                inputBytes.Clear();
            
            int bytesRead = inputFileStream.Read(inputBytes, 0, memoryLimitInBytes);
            if (bytesRead < memoryLimitInBytes)
                isLast = true;
            
            if (bytesRead == 0)
                break;
            
            int countCharsAligned = inputBytes.CountCharsAligned(bytesRead);
            if (isLast || countCharsAligned == 0 && bytesRead > 0)
                countCharsAligned = bytesRead;
            
            if (countCharsAligned == 0)
                break;
            
            parseContext = parseContext with
            {
                Iteration = iteration,
                Buffer = new MemoryBuffer(inputBytes, 0, countCharsAligned)
            };
            
            SplitIteration(parseContext);
            
            if (isLast)
                break;

            filePosition += countCharsAligned;
            iteration++;
        }
        
        if (files.Count > 1)
        {
            using var _ = new Measure("MergeFiles");

            if (command.Configuration.Sort.MergeIterative)
            {
                Queue<FileName> fileQueue = new Queue<FileName>(files);
                while (fileQueue.Count >= 2)
                {
                    var filesCount = fileQueue.Count;
                    for (int mergeIter = 0; mergeIter < filesCount; mergeIter += 2)
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
            else
            {
                var outputFile = command.BuildOutputName(files);
                using var fileMerge = new Measure($"MergeOutput: {outputFile.Name}");
                command.FileMerger.MergeFiles(files.ToArray(), outputFile);
            }
        }
        
        int end = 0;
    }

    private static void SplitIteration(ParseContext parseContext)
    {
        var command = parseContext.Command;
        var fileParser = parseContext.Command.FileParser;
        int iteration = parseContext.Iteration;
        
        using (var readAndParse = new Measure($"ReadAndParse {iteration}"))
        {
            var parseResult = fileParser.ReadAndParse(parseContext);
            readAndParse.AddInfo($"TotalLines = {parseResult.TotalLines}");
        }

        if (parseContext.VirtualTargetIndex.Count > 0)
        {
            KeyValuePair<VirtualString, List<int>>[] orderedByText;
            bool sortNumbersInPlace = true;
           
            using (new Measure("Sort"))
            {
                orderedByText = parseContext
                    .VirtualTargetIndex
                    .OrderBy(pair => pair.Key)
                    .ToArray();

                if (sortNumbersInPlace)
                {
                    // Sort in-place
                    foreach (var pair in orderedByText)
                        pair.Value.Sort();
                }
            }
            
            var fileName = new FileName(string.Format(command.OutputFileName, iteration), iteration.ToString());
            parseContext.Files.Add(fileName);
            using (new Measure($"WriteSorted {fileName.ShortFileName}"))
            {
                using var outputFileStream = new FileStream(fileName.Name, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 1.MegabytesInBytes());
                
                ReadOnlySpan<byte> dotDelimiter = Encoding.UTF8.GetBytes(". ").AsSpan();
                ReadOnlySpan<byte> newLine = Encoding.UTF8.GetBytes(Environment.NewLine).AsSpan();
                
                foreach (var linePair in orderedByText)
                {
                    var text = linePair.Key.AsSpan();
                    var orderedNumbers = linePair.Value;
                    if (!sortNumbersInPlace)
                    {
                        var numbers = new List<int>(linePair.Value.Count);
                        numbers.AddRange(linePair.Value.OrderBy(n => n));
                        orderedNumbers = numbers;
                    }

                    foreach (var number in orderedNumbers)
                    {
                        using var numberBytes = number.ToSpan();
                        outputFileStream.Write(numberBytes.AsSpan());
                        outputFileStream.Write(dotDelimiter);
                        outputFileStream.Write(text);
                        outputFileStream.Write(newLine);
                    }
                }

                outputFileStream.Flush();
            }
        }

        if (parseContext.TargetIndex.Count > 0)
        {
            KeyValuePair<string, List<int>>[] orderedByText;
            using (new Measure("Sort"))
            {
                orderedByText = parseContext.TargetIndex.OrderBy(pair => pair.Key).ToArray();
            }

            var fileName = new FileName(string.Format(command.OutputFileName, iteration), iteration.ToString());
            parseContext.Files.Add(fileName);
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
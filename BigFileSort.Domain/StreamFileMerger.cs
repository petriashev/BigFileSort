using System.Runtime.CompilerServices;

namespace BigFileSort.Domain;

public sealed class StreamFileMerger : IFileMerger
{
    private readonly BigFileSortConfiguration _configuration;
    private readonly FileStreamOptions _fileStreamOptions;

    public StreamFileMerger(BigFileSortConfiguration configuration, FileStreamOptions? fileStreamOptions = null)
    {
        _configuration = configuration;
        _fileStreamOptions = fileStreamOptions ?? new FileStreamOptions{BufferSize = 100.MegabytesInBytes(), Access = FileAccess.Read, Options = FileOptions.SequentialScan};
    }

    /// <inheritdoc />
    public void MergeFiles(FileName fileName1, FileName fileName2, FileName outputFileName)
    {
        using var file1 = new StreamReader(fileName1.Name, _fileStreamOptions);
        using var file2 = new StreamReader(fileName2.Name, _fileStreamOptions);
        using var output = new StreamWriter(outputFileName.Name);

        Line? line1 = null;
        Line? line2 = null;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WriteLine1()
        {
            output.WriteLine(line1);
            line1 = null;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WriteLine2()
        {
            output.WriteLine(line2);
            line2 = null;
        }
        
        while (true)
        {
            line1 ??= file1.ReadLine().ParseLine();
            line2 ??= file2.ReadLine().ParseLine();

            if (line1 == null && line2 != null)
            {
                WriteLine2();
                continue;
            }

            if (line1 != null && line2 == null)
            {
                WriteLine1();
                continue;
            }

            if (line1 == null && line2 == null)
            {
                break;
            }

            if (line1.Text == line2.Text)
            {
                if (line1.Number < line2.Number)
                    WriteLine1();
                else
                    WriteLine2();
                
            }
            else
            {
                if (line1 < line2)
                    WriteLine1();
                else
                    WriteLine2();
            }
        }
    }

    /// <inheritdoc />
    public void MergeFiles(FileName[] files, FileName outputFile)
    {
        StreamReader?[] readers = files.Select(name => new StreamReader(name.Name, _fileStreamOptions)).ToArray();
        var lines = new Line?[files.Length];
        var notNull = new int[files.Length];
        using var output = new StreamWriter(outputFile.Name);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WriteLine(int i)
        {
            //output.WriteLine(lines[i]);
            output.Write(lines[i].Number);
            output.Write(". ");
            output.Write(lines[i].Text);
            output.Write(Environment.NewLine);
            
            lines[i] = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int FirstNotNull()
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] != default)
                    return i;
            }

            return -1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        LineValue LineValue(int i)
        {
            var notNullLine = lines[notNull[i]]!;
            return new (notNullLine.Number, notNullLine.Text);
        }
        
        while (true)
        {
            int countLines = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == default && readers[i] != null)
                {
                    var lineFromStream = readers[i]?.ReadLine();
                    if (lineFromStream == null)
                    {
                        // Закончились строки в файле i.
                        readers[i]?.Dispose();
                        readers[i] = null;

                        if (_configuration.Sort.DeleteTempFiles)
                        {
                            try
                            {
                                File.Delete(files[i].Name);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        }
                    }
                    else
                    {
                        lines[i] = lineFromStream.ParseLine();
                    }
                }

                if (lines[i] != default)
                {
                    notNull[countLines] = i;
                    countLines++;
                }
            }
            
            if (countLines == 0)
            {
                // Закончились строки везде.
                break;
            }

            if (countLines == 1)
            {
                // Есть одна строка, ее и запишем.
                WriteLine(FirstNotNull());
                continue;
            }

            if (countLines == 2)
            {
                var i1 = notNull[0];
                var i2 = notNull[1];
                
                var line1 = lines[i1];
                var line2 = lines[i2];
                
                if (line1.Text == line2.Text)
                {
                    WriteLine(line1.Number < line2.Number ? i1 : i2);
                }
                else
                {
                    WriteLine(line1 < line2 ? i1 : i2);
                }
            }
            
            if (countLines > 2)
            {
                int minLineIndex = 0;
                var minLine = LineValue(minLineIndex);
                
                for (int i = 1; i < countLines; i++)
                {
                    var current = LineValue(i);
                    if (current.Text == minLine.Text)
                    {
                        if (current.Number < minLine.Number)
                        {
                            minLine = current;
                            minLineIndex = i;
                        }
                    }
                    else if (current < minLine)
                    {
                        minLine = current;
                        minLineIndex = i;
                    }
                }
                WriteLine(notNull[minLineIndex]);
            }
        }
    }
}
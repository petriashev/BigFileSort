namespace BigFileSort.Domain;

public sealed class StreamReaderParser : IFileParser
{
    /// <inheritdoc />
    public ParseResult ReadAndParse(ParseContext parseContext)
    {
        var sourceBuffer = parseContext.SourceBuffer;
        using var streamReader = new StreamReader(sourceBuffer);
     
        int totalLines = 0;
        while (streamReader.ReadLine() is { } lineUnparsed)
        {
            // Example: '415. Apple'
            var line = lineUnparsed.ParseLineValue();
            if (!line.IsValid())
                continue;

            var text = line.Text;
            var number = line.Number;
            parseContext.AddLineToIndex(text, number);

            totalLines++;
        }

        return new ParseResult(totalLines);
    }
}

public sealed class StreamReaderParser2 : IFileParser
{
    /// <inheritdoc />
    public ParseResult ReadAndParse(ParseContext parseContext)
    {
        var sourceBuffer = parseContext.SourceBuffer;
        using var streamReader = new StreamReader(sourceBuffer);
        var textBlock = streamReader.ReadToEnd();
        var textSpan = textBlock.AsSpan();

        int startIndex = 0;
        int consumed = 0;
        
        int totalLines = 0;
        while (TryReadLine(textSpan, parseContext.Command.Delimiter, ref startIndex, ref consumed, out var line))
        {
            if (!line.IsValid())
                continue;

            var text = line.Text;
            var number = line.Number;
            parseContext.AddLineToIndex(text, number);

            totalLines++;
        }

        return new ParseResult(totalLines);
    }
    
    private bool TryReadLine(ReadOnlySpan<char> buffer, byte[] delimiter, ref int startIndex, ref int bytesConsumed, out LineValue line)
    {
        int advance = 0;
        
        int delimiterIndex = buffer.PositionOf(delimiter[0], startIndex);
        if (delimiterIndex < 0)
        {
            line = default;
            return false;
        }
        advance++;
    
        if (delimiter.Length > 1)
        {
            bool isInRange = delimiterIndex + 1 < buffer.Length - 1;
            if (isInRange && buffer[delimiterIndex + 1] != delimiter[1])
            {
                line = default;
                return false;
            }

            advance++;
        }
        
        var length = delimiterIndex - startIndex;
        line = LineParser.FromCharSpan(buffer.Slice(startIndex, length));
        
        startIndex = delimiterIndex + advance;
        bytesConsumed += length + advance;
        
        return true;
    }
}
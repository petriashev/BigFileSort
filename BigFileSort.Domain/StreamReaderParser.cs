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
using System.Buffers;
using System.IO.Pipelines;

namespace BigFileSort.Domain;

public sealed class SpanFileParser
{
    /// <inheritdoc />
    public async Task<ParseResult> ReadAndParse(ParseContext parseContext)
    {
        var sourceBuffer = parseContext.SourceBuffer;
        byte[] delimiter = parseContext.Command.Delimiter;
        
        var pipeReader = PipeReader.Create(sourceBuffer);
        int totalLines = 0;
        while (true)
        {
            var result = await pipeReader.ReadAsync();
            ReadOnlySequence<byte> buffer = result.Buffer;
            
            while (TryReadLine(ref buffer, out ReadOnlySequence<byte> sequence, delimiter))
            {
                LineValue line = sequence.Parse();
                // if (!line.IsValid())
                //     continue;
                
                var text = line.Text;
                var number = line.Number;
                parseContext.AddLineToIndex(text, number);
                
                totalLines++;
            }
            
            pipeReader.AdvanceTo(buffer.Start, buffer.End);
            if (result.IsCompleted)
            {
                break;
            }
        }
        
        return new ParseResult(totalLines);
    }
    
    private bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line, byte[] delimiter)
    {
        var position = buffer.PositionOf(delimiter[0]);
        if (position == null)
        {
            line = default;
            return false;
        }
        
        var position2 = buffer.PositionOf(delimiter[1]);
        if (position2 == null)
        {
            line = default;
            return false;
        }
        
        line = buffer.Slice(0, position.Value);
        buffer = buffer.Slice(buffer.GetPosition(delimiter.Length, position.Value));

        return true;
    }
}

public static class SpanParser
{
    public static LineValue Parse(this in ReadOnlySequence<byte> sequence)
    {
        if (sequence.IsSingleSegment)
        {
            return sequence.FirstSpan.ParseLine();
        }
        
        var length = (int) sequence.Length;
        if (length > 4000)
        {
            throw new ArgumentException($"Line has a length exceeding the limit: {length}");
        }

        Span<byte> span = stackalloc byte[(int)sequence.Length];
        sequence.CopyTo(span);

        return LineParser.ParseLine(span);
    }
}
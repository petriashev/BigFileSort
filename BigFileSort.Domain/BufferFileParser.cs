using System.Text;

namespace BigFileSort.Domain;

public sealed class BufferFileParser : IFileParser
{
    /// <inheritdoc />
    public ParseResult ReadAndParse(ParseContext parseContext)
    {
        var buffer = parseContext.Buffer;
        byte[] delimiter = parseContext.Command.Delimiter;
        Encoding? encoding = parseContext.Command.Encoding;

        int totalLines = 0;
        int startIndex = 0;
        int bytesConsumed = 0;

        while (TryReadLine(buffer, delimiter, encoding, ref startIndex, ref bytesConsumed, out LineValue line))
        {
            if (!line.IsValid())
                continue;
            
            parseContext.AddLineToIndex(line.Text, line.Number);
            
            totalLines++;
        }

        if (bytesConsumed < parseContext.BufferLength)
        {
            int length = parseContext.BufferLength - bytesConsumed;
            if (TryReadLastLine(buffer, encoding, ref startIndex, length, out LineValue line))
            {
                parseContext.AddLineToIndex(line.Text, line.Number);
                totalLines++;
            }
        }

        return new ParseResult(totalLines);
    }

    private bool TryReadLine(byte[] buffer, byte[] delimiter, Encoding? encoding, ref int startIndex, ref int bytesConsumed, out LineValue line)
    {
        int advance = 0;
        
        int position = buffer.PositionOf(delimiter[0], startIndex);
        if (position < 0)
        {
            line = default;
            return false;
        }
        advance++;
    
        if (delimiter.Length > 1)
        {
            bool isInRange = position + 1 < buffer.Length - 1;
            if (isInRange && buffer[position+1] != delimiter[1])
            {
                line = default;
                return false;
            }

            advance++;
        }
        
        var length = position - startIndex;
        line = LineParser.FromByteBuffer(buffer, startIndex, length, encoding);
        
        startIndex = position + advance;
        bytesConsumed += length + advance;
            
        return true;
    }
    
    private bool TryReadLastLine(byte[] buffer, Encoding? encoding, ref int startIndex, int length, out LineValue line)
    {
        line = buffer.ParseLine(startIndex, length, encoding);
        return true;
    }
}
using System.Text;

namespace BigFileSort.Domain;

public sealed class BufferFileParser : IFileParser
{
    /// <inheritdoc />
    public ParseResult ReadAndParse(ParseContext parseContext)
    {
        ParserState state = new ParserState(parseContext);
        
        while (state.TryReadLine(isLastLine: false))
        {
            SplitAndAddToIndex(parseContext, state);
        }
        
        if (state.BytesConsumed < state.Buffer.Length)
        {
            if (state.TryReadLine(isLastLine: true))
            {
                SplitAndAddToIndex(parseContext, state);
            }
        }

        return new ParseResult(state.TotalLines);
    }
    
    private static void SplitAndAddToIndex(ParseContext parseContext, ParserState state)
    {
        var line = new VirtualString(state.Buffer, state.StartIndex, state.Length);
        if (Split(line, out var number, out var text))
        {
            parseContext.AddLineToIndex(text, number.ToInt());
        }
        else
        {
            Console.WriteLine($"Split error. Line {state.TotalLines}");
        }
    }
    
    private static bool Split(in VirtualString virtualString, out VirtualString number, out VirtualString text)
    {
        int dotIndexAbs = virtualString.Buffer.PositionOf((byte)'.', virtualString.Start);
        int textIndexAbs = dotIndexAbs + 2;
        int textIndex = textIndexAbs - virtualString.Start;
        int dotIndex = dotIndexAbs - virtualString.Start;
        
        number = new VirtualString(virtualString.Buffer, virtualString.Start, dotIndex);
        text = new VirtualString(virtualString.Buffer, textIndexAbs, virtualString.Length - textIndex);
            
        return number.Length > 0 && text.Length > 0;
    }
}

public sealed class ParserState
{
    public ParseContext Context { get; }
    public MemoryBuffer Buffer => Context.Buffer;
    public byte[] Delimiter => Context.Command.Delimiter;
    
    public ParserState(ParseContext parseContext)
    {
        Context = parseContext;
    }
    
    public int CurrentIndex;
    public int StartIndex;
    public int Length;
    
    public int BytesConsumed;
    public int TotalLines;
}

public static class BufferParser
{
    public static bool TryReadLine(this ParserState state, bool isLastLine)
    {
        var buffer = state.Buffer.AsSpan();
        byte[] delimiter = state.Delimiter;
        
        #region Delimiter search
        int advance = 0;
        
        int delimiterIndex;
        if (!isLastLine)
        {
            delimiterIndex = buffer.PositionOf(delimiter[0], state.CurrentIndex);
            if (delimiterIndex < 0)
            {
                state.Length = 0;
                return false;
            }
            advance++;

            if (delimiter.Length > 1)
            {
                bool isInRange = delimiterIndex + 1 <= buffer.Length - 1;
                if (isInRange)
                {
                    if (buffer[delimiterIndex+1] != delimiter[1])
                    {
                        state.Length = 0;
                        return false;
                    }
                    advance++;
                }
            }       
        }
        else
        {
            delimiterIndex = buffer.PositionOf(0, state.CurrentIndex);
            if(delimiterIndex < 0)
                delimiterIndex = buffer.Length - 1;
        }
        
        #endregion
        
        // current line start and length
        state.StartIndex = state.CurrentIndex;
        state.Length = delimiterIndex - state.StartIndex;
        
        // next line start
        state.CurrentIndex = delimiterIndex + advance;

        state.BytesConsumed += state.Length + advance;
        state.TotalLines += 1;
        
        return state.Length > 0;
    }
}
using System.Runtime.CompilerServices;
using System.Text;

namespace BigFileSort.Domain;

public static class LineParser
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValid(this Line? line) => line?.Text != null;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValid(this in LineValue line) => line.Text != null;
    
    public static LineValue FromByteBuffer(byte[] source, int startIndex, int length, Encoding? encoding)
    {
        Span<char> chars = stackalloc char[length];
        encoding ??= Encoding.Default;
        encoding.GetChars(source.AsSpan(startIndex, length), chars);
        return FromCharSpan(chars);
    }
    
    public static LineValue FromByteBufferUnsafe(byte[] source, int startIndex, int length)
    {
        int dotIndexAbs = source.PositionOf((byte)'.', startIndex);
        int textIndexAbs = dotIndexAbs + 2;
        int textIndex = textIndexAbs - startIndex;
        
        var number = int.Parse(source.ToStringUnsafe(startIndex, dotIndexAbs - startIndex));
        var text = source.ToStringUnsafe(textIndexAbs, length - textIndex);
        return new LineValue(number, text);
    }
    
    public static LineValue FromCharBuffer(char[] source, int startIndex, int length)
    {
        int dotIndexAbs = source.PositionOf((byte)'.', startIndex);
        int textIndexAbs = dotIndexAbs + 2;
        int textIndex = textIndexAbs - startIndex;
        int dotIndex = dotIndexAbs - startIndex;
        
        var number = int.Parse(source.AsSpan()[..dotIndex]);
        var text = new string(source, textIndex, length - textIndex);
        return new LineValue(number, text);
    }
    
    public static LineValue FromByteSpan(in ReadOnlySpan<byte> source, Encoding? encoding = null)
    {
        Span<char> chars = stackalloc char[source.Length];
        encoding ??= Encoding.Default;
        encoding.GetChars(source, chars);
        return FromCharSpan(chars);
    }
    
    public static LineValue FromCharSpan(in ReadOnlySpan<char> source)
    {
        var dotIndex = source.IndexOf('.');
        var textIndex = dotIndex + 2;
            
        var number = int.Parse(source[..dotIndex]);
        var textLength = source.Length - textIndex;
        var text = source.Slice(textIndex, textLength).ToString();

        return new LineValue(number, text);
    }
    
    public static LineValue ParseLine(this byte[] bytes, int startIndex, int length, Encoding? encoding)
        => FromByteBuffer(bytes, startIndex, length, encoding);

    public static LineValue ParseLine(this in ReadOnlySpan<byte> bytes, Encoding? encoding = null)
        => FromByteSpan(bytes, encoding);
    
    public static LineValue ParseLineUnsafe(this byte[] bytes, int startIndex, int length)
        => FromByteBufferUnsafe(bytes, startIndex, length);
    
    public static LineValue? TryParseLineValue(this string? line)
        => line?.ParseLineValue();

    public static LineValue ParseLineValue(this string line)
        => FromCharSpan(line.AsSpan());

    public static Line? ParseLine(this string? line)
    {
        if (line != null)
        {
            ReadOnlySpan<char> span = line.AsSpan();
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] == '.')
                {
                    int dotIndex = i;
                    int textStart = dotIndex + 2;
                    int number = int.Parse(span.Slice(0, dotIndex));
                    string text = span.Slice(textStart, span.Length - textStart).ToString();
                    return new Line(line, number, text);
                }
            }
        }
        
        return null;
    }
}
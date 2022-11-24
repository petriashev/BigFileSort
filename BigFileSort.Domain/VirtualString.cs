using System.Runtime.CompilerServices;
using System.Text;

namespace BigFileSort.Domain;

public readonly struct VirtualString : IEquatable<VirtualString>, IComparable<VirtualString>
{
    public byte[] Buffer { get; }
    public int Start { get; }
    public int Length { get; }

    public VirtualString(byte[] buffer, int start, int length)
    {
        Buffer = buffer;
        Start = start;
        Length = length;
    }
    
    public VirtualString(MemoryBuffer buffer, int start, int length)
    {
        Buffer = buffer.Buffer;
        Start = buffer.Start + start;
        Length = length;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> AsSpan() => new (Buffer, Start, Length);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ToInt() => AsSpan().ToInt();

    public static implicit operator ReadOnlySpan<byte>(VirtualString value) => value.AsSpan();
    
    public static bool operator < (VirtualString line1, VirtualString line2) => SpanComparer.CompareAsString(line1, line2) < 0;

    public static bool operator > (VirtualString line1, VirtualString line2) => SpanComparer.CompareAsString(line1, line2) > 0;
    
    public static bool operator == (VirtualString line1, VirtualString line2) => SpanComparer.CompareAsString(line1, line2) == 0;

    public static bool operator !=(VirtualString line1, VirtualString line2) => !(line1 == line2);

    /// <inheritdoc />
    public bool Equals(VirtualString other) => SpanComparer.CompareAsString(this, other) == 0;

    /// <inheritdoc />
    public int CompareTo(VirtualString other) => SpanComparer.CompareAsString(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is VirtualString other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.AddBytes(AsSpan());
        return hashCode.ToHashCode();
    }
    
    /// <summary> Returns string representation. </summary>
    public string AsString() => Encoding.UTF8.GetString(AsSpan());
    
    /// <inheritdoc />
    public override string ToString() => AsString();
}

public static class VirtualStringComparer
{
    public static IComparer<VirtualString> Ordinal { get; } = new VirtualStringOrdinalComparer();
    public static IComparer<VirtualString> ValueAsNumber { get; } = new ValueVirtualStringAsNumberComparer();
}

public sealed class VirtualStringOrdinalComparer : IComparer<VirtualString>
{
    /// <inheritdoc />
    public int Compare(VirtualString x, VirtualString y) => SpanComparer.CompareAsString(x, y);
}

public sealed class ValueVirtualStringAsNumberComparer : IComparer<VirtualString>
{
    /// <inheritdoc />
    public int Compare(VirtualString x, VirtualString y) => SpanComparer.CompareAsNumber(x, y);
}

public static class SpanComparer
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CompareAsString(in ReadOnlySpan<byte> span1, in ReadOnlySpan<byte> span2)
    {
        if (span1.Length == 0 && span2.Length == 0) return 0;
        int max = Math.Min(span1.Length, span2.Length);
        for (int i = 0; i < max; i++)
        {
            if (span1[i] < span2[i])
                return -1;
            if (span1[i] > span2[i])
                return 1;
        }
        return span1.Length.CompareTo(span2.Length);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CompareAsNumber(in ReadOnlySpan<byte> span1, in ReadOnlySpan<byte> span2)
    {
        if (span1.Length == 0 && span2.Length == 0)
            return 0;
        if (span1.Length < span2.Length)
            return -1;
        if (span1.Length > span2.Length)
            return 1;
        
        for (int i = 0; i < span1.Length; i++)
        {
            if (span1[i] < span2[i])
                return -1;
            if (span1[i] > span2[i])
                return 1;
        }
        return 0;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToInt(this in ReadOnlySpan<byte> span)
    {
        int number = 0;
        for (var i = 0; i < span.Length; i++)
        {
            //48:0-57:9
            var b = span[i];
            int d = b - 48;
            number += d * Pow10(span.Length-i-1);
        }
        
        return number;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Pow10(int n)
    {
        if (n == 0) return 1;
        if (n == 1) return 10;
        if (n == 2) return 100;
            
        int pow = 1;
        for (int i = 0; i < n; i++)
            pow *= 10;
            
        return pow;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountDigits(uint value)
    {
        int digits = 1;
        if (value >= 100000)
        {
            value /= 100000;
            digits += 5;
        }

        if (value < 10)
        {
            // no-op
        }
        else if (value < 100)
        {
            digits++;
        }
        else if (value < 1000)
        {
            digits += 2;
        }
        else if (value < 10000)
        {
            digits += 3;
        }
        else
        {
            digits += 4;
        }

        return digits;
    }

    public static RentedBuffer<byte> ToSpan(this int number)
    {
        int bufferLength = CountDigits((uint)number);
        var buffer = new RentedBuffer<byte>(bufferLength);
        
        int current = number;
        for (int i = 0; i < bufferLength; i++)
        {
            var res = Math.DivRem(current, 10);
            buffer.Buffer[bufferLength - i - 1] = (byte)(res.Remainder + 48);
            current = res.Quotient;
        }

        return buffer;
    }
}
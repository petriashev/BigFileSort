using System.Runtime.CompilerServices;

namespace BigFileSort.Domain;

public static class BufferExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PositionOf(this byte[] buffer, byte value, int startIndex = 0)
    {
        for (var i = startIndex; i < buffer.Length; i++)
            if (buffer[i] == value)
                return i;

        return -1;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PositionOf(this char[] buffer, byte value, int startIndex = 0)
    {
        for (var i = startIndex; i < buffer.Length; i++)
            if (buffer[i] == value)
                return i;

        return -1;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PositionOf(this in ReadOnlySpan<char> buffer, byte value, int startIndex = 0)
    {
        for (var i = startIndex; i < buffer.Length; i++)
            if (buffer[i] == value)
                return i;

        return -1;
    }
    
    internal static string ToStringUnsafe(this byte[] bytes, int start, int length)
    {
        unsafe
        {
            string returnStr;
            fixed(byte* fixedPtr = bytes)
            {
                returnStr = new string((sbyte*)fixedPtr, start, length);
            }
            return returnStr;
        }
    }
    
    public static RentedBuffer<char> ToCharArray(this byte[] bytes)
    {
        var rentedBuffer = new RentedBuffer<char>(bytes.Length);
        
        for (int i = 0; i < bytes.Length; i++)
        {
            rentedBuffer.Buffer[i] = (char)bytes[i];
        }

        return rentedBuffer;
    }
}
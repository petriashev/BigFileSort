﻿using System.Runtime.CompilerServices;

namespace BigFileSort.Domain;

internal static class BufferExtensions
{
    public static int PositionOf(this byte[] buffer, byte value, int startIndex = 0)
    {
        for (var i = startIndex; i < buffer.Length; i++)
            if (buffer[i] == value)
                return i;

        return -1;
    }
    
    public static int PositionOf(this char[] buffer, byte value, int startIndex = 0)
    {
        for (var i = startIndex; i < buffer.Length; i++)
            if (buffer[i] == value)
                return i;

        return -1;
    }
    
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
    
    internal static Span<char> ToCharArrayUnsafe(this byte[] bytes)
    {
        unsafe
        {
            fixed(byte* fixedPtr = &bytes[0])
            {
                char* charPtr = (char*)fixedPtr;

                return new Span<char>(charPtr, bytes.Length);
            }
        }
    }
}
using System.Runtime.CompilerServices;

namespace BigFileSort.System;

public static class UnitConverter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long GigabytesInBytes(this int gigabytes) => (long)gigabytes * 1024 * 1024 * 1024;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int MegabytesInBytes(this int megabytes) => megabytes * 1024 * 1024;

    public static long GetDataSizeInBytes(this int dataSize, DataUnit dataUnit)
    {
        return dataUnit switch
        {
            DataUnit.BYTES => dataSize,
            DataUnit.MEGABYTES => dataSize * 1024 * 1024,
            DataUnit.GIGABYTES => (long)dataSize * 1024 * 1024 * 1024,
            _ => dataSize
        };
    }
}

public enum DataUnit
{
    BYTES,
    MEGABYTES,
    GIGABYTES
}
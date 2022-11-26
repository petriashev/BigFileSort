using System.Runtime.CompilerServices;

namespace BigFileSort.Domain;

public static class UnitConverter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long GigabytesInBytes(this int gigabytes) => (long)gigabytes * 1024 * 1024 * 1024;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int MegabytesInBytes(this int megabytes) => megabytes * 1024 * 1024;
}
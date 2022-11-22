namespace BigFileSort.Domain;

public static class UnitConverter
{
    public static long GigabytesInBytes(this int gigabytes) => (long)gigabytes * 1024 * 1024 * 1024;
    public static int MegabytesInBytes(this int megabytes) => megabytes * 1024 * 1024;
}
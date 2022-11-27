namespace BigFileSort.System;

public sealed record MemoryBuffer(byte[] Buffer, int Start, int Length)
{
    public ReadOnlySpan<byte> AsSpan() => new ReadOnlySpan<byte>(Buffer, Start, Length);
    
    public MemoryStream AsMemoryStream() => new MemoryStream(Buffer, Start, Length);
    
    public static implicit operator ReadOnlySpan<byte>(MemoryBuffer value) => value.AsSpan();
}
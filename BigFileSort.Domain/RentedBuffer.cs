using System.Buffers;

namespace BigFileSort.Domain;

public readonly struct RentedBuffer<T> : IDisposable
{
    public readonly T[] Buffer;
    public readonly int Length;
    
    public RentedBuffer(int length)
    {
        Length = length;
        Buffer = ArrayPool<T>.Shared.Rent(length);
    }

    public ReadOnlySpan<T> AsSpan() => new (Buffer, 0, Length);
    
    public static implicit operator ReadOnlySpan<T>(RentedBuffer<T> rentedBuffer) => rentedBuffer.AsSpan();

    /// <summary>
    /// Returns rented buffer to pool.
    /// </summary>
    public void Return() => ArrayPool<T>.Shared.Return(Buffer);

    /// <inheritdoc />
    public void Dispose() => Return();

    /// <inheritdoc />
    public override string ToString() => AsSpan().ToString();
}
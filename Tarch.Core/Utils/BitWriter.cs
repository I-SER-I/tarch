namespace Itmo.Fitp.Is.Tarch.Core.Utils;

public sealed class BitWriter(Stream stream) : IDisposable
{
    private readonly Stream _stream = stream ?? throw new ArgumentNullException(nameof(stream));
    private byte _buffer;
    private int _bitCount;

    public void WriteBits((uint Code, int Length) code) => WriteBits(code.Code, code.Length);

    private void WriteBits(uint value, int bitCount)
    {
        for (int i = bitCount - 1; i >= 0; i--)
        {
            var bit = ((value >> i) & 1) == 1;
            WriteBit(bit);
        }
    }

    private void WriteBit(bool bit)
    {
        if (bit)
        {
            _buffer |= (byte)(1 << (7 - _bitCount));
        }

        _bitCount++;

        if (_bitCount == 8)
        {
            FlushByte();
        }
    }

    private void FlushByte()
    {
        _stream.WriteByte(_buffer);
        _buffer = 0;
        _bitCount = 0;
    }

    public void Flush()
    {
        if (_bitCount > 0)
        {
            FlushByte();
        }

        _stream.Flush();
    }

    public void Dispose() => Flush();
}
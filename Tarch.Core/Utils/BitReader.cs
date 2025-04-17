namespace Itmo.Fitp.Is.Tarch.Core.Utils;

public sealed class BitReader
{
    private readonly Stream _stream;
    private int _buffer;
    private int _bitCount;

    public bool HasBits => _bitCount > 0 || _stream.Position < _stream.Length;

    public BitReader(Stream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        FillBuffer();
    }

    private void FillBuffer()
    {
        int nextByte = _stream.ReadByte();
        if (nextByte == -1)
        {
            _buffer = 0;
            _bitCount = 0;
        }
        else
        {
            _buffer = nextByte;
            _bitCount = 8;
        }
    }

    public int ReadBit()
    {
        if (_bitCount == 0)
            FillBuffer();

        if (_bitCount == 0)
            throw new EndOfStreamException("No more bits to read.");

        int bit = (_buffer >> (--_bitCount)) & 1;
        return bit;
    }
}
namespace Itmo.Fitp.Is.Tarch.Core.Utils;

public sealed class BitWriter(Stream stream) : IDisposable
{
    private byte _currentByte;
    private int _bitCount;

    public void WriteBits(IEnumerable<bool> bits)
    {
        foreach (var bit in bits)
        {
            _currentByte <<= 1;
            if (bit)
            {
                _currentByte |= 1;
            }

            _bitCount++;

            if (_bitCount == Constants.BitsPerByte)
            {
                FlushByte();
            }
        }
    }

    public void Flush()
    {
        if (_bitCount > 0)
        {
            _currentByte <<= (Constants.BitsPerByte - _bitCount);
            stream.WriteByte(_currentByte);
            _bitCount = 0;
            _currentByte = 0;
        }
    }

    private void FlushByte()
    {
        stream.WriteByte(_currentByte);
        _bitCount = 0;
        _currentByte = 0;
    }

    public void Dispose() => Flush();
}

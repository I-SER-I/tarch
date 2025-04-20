namespace Itmo.Fitp.Is.Tarch.Core.Utils;

public sealed class BitReader(Stream stream)
{
    private int _currentByte;
    private int _bitPosition = Constants.BitsPerByte;

    public bool ReadBit()
    {
        if (_bitPosition == Constants.BitsPerByte)
        {
            _currentByte = stream.ReadByte();
            if (_currentByte == Constants.EndOfStream)
            {
                throw new EndOfStreamException("Unexpected end of stream while reading bits.");
            }

            _bitPosition = 0;
        }

        var bit = ((_currentByte >> (7 - _bitPosition)) & 1) == 1;
        _bitPosition++;
        return bit;
    }
}

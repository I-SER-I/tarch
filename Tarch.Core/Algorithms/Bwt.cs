namespace Itmo.Fitp.Is.Tarch.Core.Algorithms;

public sealed class Bwt : IAlgorithm
{
    public void Encode(Stream input, Stream output)
    {
        using var reader = new BinaryReader(input, Encoding.UTF8, leaveOpen: true);
        using var writer = new BinaryWriter(output, Encoding.UTF8, leaveOpen: true);

        byte[] data = reader.ReadBytes((int)(input.Length - input.Position));
        var (bwt, index) = BurrowsWheelerTransform(data);

        writer.Write(index);
        var mtf = MoveToFront(bwt);
        var rle = RunLengthEncode(mtf);

        writer.Write(rle.Length);
        writer.Write(rle);
    }

    public void Decode(Stream input, Stream output)
    {
        using var reader = new BinaryReader(input, Encoding.UTF8, leaveOpen: true);
        using var writer = new BinaryWriter(output, Encoding.UTF8, leaveOpen: true);

        int index = reader.ReadInt32();
        int len = reader.ReadInt32();
        byte[] rle = reader.ReadBytes(len);

        var mtf = RunLengthDecode(rle);
        var bwt = MoveToFrontDecode(mtf);
        var original = InverseBurrowsWheelerTransform(bwt, index);

        writer.Write(original);
    }

    // ==== BWT ====
    private (byte[] result, int index) BurrowsWheelerTransform(byte[] input)
    {
        int n = input.Length;
        var rotations = new List<string>(n);

        string s = Encoding.UTF8.GetString(input);
        for (int i = 0; i < n; i++)
            rotations.Add(s.Substring(i) + s.Substring(0, i));

        var sorted = rotations.Select((x, i) => (x, i)).OrderBy(x => x.x).ToArray();

        var lastColumn = sorted.Select(x => x.x[^1]).ToArray();
        int index = Array.FindIndex(sorted, x => x.i == 0);
        return (Encoding.UTF8.GetBytes(lastColumn), index);
    }

    private byte[] InverseBurrowsWheelerTransform(byte[] bwt, int index)
    {
        int n = bwt.Length;
        var table = Enumerable.Repeat(string.Empty, n).ToArray();

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                table[j] = (char)bwt[j] + table[j];

            Array.Sort(table);
        }

        return Encoding.UTF8.GetBytes(table[index]);
    }

    // ==== Move-To-Front ====
    private byte[] MoveToFront(byte[] input)
    {
        List<byte> symbols = Enumerable.Range(0, 256).Select(i => (byte)i).ToList();
        List<byte> result = new();

        foreach (byte b in input)
        {
            int idx = symbols.IndexOf(b);
            result.Add((byte)idx);
            symbols.RemoveAt(idx);
            symbols.Insert(0, b);
        }

        return result.ToArray();
    }

    private byte[] MoveToFrontDecode(byte[] input)
    {
        List<byte> symbols = Enumerable.Range(0, 256).Select(i => (byte)i).ToList();
        List<byte> result = new();

        foreach (byte i in input)
        {
            byte sym = symbols[i];
            result.Add(sym);
            symbols.RemoveAt(i);
            symbols.Insert(0, sym);
        }

        return result.ToArray();
    }

    // ==== RLE ====
    private byte[] RunLengthEncode(byte[] input)
    {
        List<byte> output = new();
        for (int i = 0; i < input.Length;)
        {
            byte current = input[i];
            int count = 1;
            while (i + count < input.Length && input[i + count] == current && count < 255)
                count++;

            output.Add(current);
            output.Add((byte)count);
            i += count;
        }
        return output.ToArray();
    }

    private byte[] RunLengthDecode(byte[] input)
    {
        List<byte> output = new();
        for (int i = 0; i < input.Length; i += 2)
        {
            byte val = input[i];
            byte count = input[i + 1];
            for (int j = 0; j < count; j++)
                output.Add(val);
        }
        return output.ToArray();
    }
}

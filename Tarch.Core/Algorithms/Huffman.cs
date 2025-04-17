namespace Itmo.Fitp.Is.Tarch.Core.Algorithms;

public class Huffman : IAlgorithm
{
    private readonly Dictionary<byte, string> _codes = new();

    public void Encode(Stream input, Stream output)
    {
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        byte[] data = ms.ToArray();

        var frequency = new Dictionary<byte, int>();
        foreach (var b in data)
            frequency[b] = frequency.GetValueOrDefault(b) + 1;

        var root = BuildTree(frequency);

        _codes.Clear();
        BuildCodeTable(root, "");

        // 1. Таблица частот
        for (int i = 0; i < 256; i++)
        {
            byte symbol = (byte)i;
            frequency.TryGetValue(symbol, out int freq);
            output.Write(BitConverter.GetBytes(freq));
        }

        // 2. Длина исходных данных
        output.Write(BitConverter.GetBytes(data.Length));

        // 3. Кодировка в биты
        var bits = new List<bool>();
        foreach (var b in data)
            bits.AddRange(_codes[b].Select(c => c == '1'));

        // 4. Записываем количество бит
        output.Write(BitConverter.GetBytes(bits.Count));

        // 5. Упаковываем биты в байты
        var bitArray = new BitArray(bits.ToArray());
        byte[] encoded = new byte[(bitArray.Length + 7) / 8];
        bitArray.CopyTo(encoded, 0);
        output.Write(encoded, 0, encoded.Length);
    }

    public void Decode(Stream input, Stream output)
    {
        var frequency = new Dictionary<byte, int>();
        for (int i = 0; i < 256; i++)
        {
            byte[] freqBytes = new byte[4];
            input.Read(freqBytes, 0, 4);
            int freq = BitConverter.ToInt32(freqBytes, 0);
            if (freq > 0)
                frequency[(byte)i] = freq;
        }

        byte[] lengthBytes = new byte[4];
        input.Read(lengthBytes, 0, 4);
        int originalLength = BitConverter.ToInt32(lengthBytes, 0);

        byte[] bitLengthBytes = new byte[4];
        input.Read(bitLengthBytes, 0, 4);
        int bitLength = BitConverter.ToInt32(bitLengthBytes, 0);

        using var encodedStream = new MemoryStream();
        input.CopyTo(encodedStream);
        byte[] encodedBytes = encodedStream.ToArray();

        var bits = new BitArray(encodedBytes);

        var trimmedBits = new bool[bitLength];
        for (int i = 0; i < bitLength; i++)
        {
            trimmedBits[i] = bits[i];
        }

        var trimmedBitArray = new BitArray(trimmedBits);

        var root = BuildTree(frequency);
        var node = root;
        int decoded = 0;
        for (int i = 0; i < trimmedBitArray.Length && decoded < originalLength; i++)
        {
            node = trimmedBitArray[i] ? node.Right! : node.Left!;
            if (node.IsLeaf)
            {
                output.WriteByte(node.Symbol!.Value);
                node = root;
                decoded++;
            }
        }
    }

    private HuffmanNode BuildTree(Dictionary<byte, int> frequency)
    {
        var pq = new PriorityQueue<HuffmanNode, int>();
        foreach (var kvp in frequency)
            pq.Enqueue(new HuffmanNode { Symbol = kvp.Key, Frequency = kvp.Value }, kvp.Value);

        while (pq.Count > 1)
        {
            var left = pq.Dequeue();
            var right = pq.Dequeue();
            pq.Enqueue(new HuffmanNode
            {
                Left = left,
                Right = right,
                Frequency = left.Frequency + right.Frequency
            }, left.Frequency + right.Frequency);
        }

        return pq.Dequeue();
    }

    private void BuildCodeTable(HuffmanNode node, string code)
    {
        if (node.IsLeaf)
            _codes[node.Symbol!.Value] = code;
        else
        {
            BuildCodeTable(node.Left!, code + "0");
            BuildCodeTable(node.Right!, code + "1");
        }
    }
}

internal sealed class HuffmanNode
{
    public byte? Symbol { get; set; }
    public int Frequency { get; set; }
    public HuffmanNode? Left { get; set; }
    public HuffmanNode? Right { get; set; }

    public bool IsLeaf => Left == null && Right == null;
}
namespace Itmo.Fitp.Is.Tarch.Core.Algorithms;

public sealed class ContextualHuffman : IAlgorithm
{
    private const byte StartSymbol = 0;

    public void Encode(Stream input, Stream output)
    {
        using var reader = new BinaryReader(input, Encoding.UTF8, leaveOpen: true);
        using var writer = new BinaryWriter(output, Encoding.UTF8, leaveOpen: true);

        var data = reader.ReadBytes((int)(input.Length - input.Position));
        var contextTrees = BuildHuffmanTrees(data);

        WriteCodeTable(writer, contextTrees);

        var bitWriter = new BitWriter(output);
        byte prev = StartSymbol;

        foreach (var b in data)
        {
            var code = contextTrees[prev].EncodingTable[b];
            bitWriter.WriteBits(code);
            prev = b;
        }

        bitWriter.Flush();
    }

    public void Decode(Stream input, Stream output)
    {
        using var reader = new BinaryReader(input, Encoding.UTF8, leaveOpen: true);
        using var writer = new BinaryWriter(output, Encoding.UTF8, leaveOpen: true);
        var contextTrees = ReadCodeTable(reader);
        
        var bitReader = new BitReader(input);
        byte prev = StartSymbol;

        while (bitReader.HasBits)
        {
            var symbol = contextTrees[prev].DecodeSymbol(bitReader);
            writer.Write(symbol);
            prev = symbol;
        }
    }

    private static Dictionary<byte, HuffmanModel> BuildHuffmanTrees(byte[] data)
    {
        var frequencies = new Dictionary<byte, Dictionary<byte, int>>();

        byte prev = StartSymbol;
        foreach (var b in data)
        {
            if (!frequencies.TryGetValue(prev, out var dict))
                dict = frequencies[prev] = new Dictionary<byte, int>();

            if (!dict.TryAdd(b, 1))
                dict[b]++;
            prev = b;
        }

        return frequencies.ToDictionary(
            kvp => kvp.Key,
            kvp => HuffmanModel.Build(kvp.Value)
        );
    }

    private static void WriteCodeTable(BinaryWriter writer, Dictionary<byte, HuffmanModel> models)
    {
        writer.Write(models.Count);
        foreach (var (context, model) in models)
        {
            writer.Write(context);
            model.Serialize(writer);
        }
    }

    private static Dictionary<byte, HuffmanModel> ReadCodeTable(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        var models = new Dictionary<byte, HuffmanModel>(count);

        for (int i = 0; i < count; i++)
        {
            byte context = reader.ReadByte();
            var model = HuffmanModel.Deserialize(reader);
            models[context] = model;
        }

        return models;
    }
}

internal sealed class HuffmanModel
{
    public Dictionary<byte, (uint Code, int Length)> EncodingTable { get; }
    private Node Root { get; }

    private HuffmanModel(Node root, Dictionary<byte, (uint, int)> table)
    {
        Root = root;
        EncodingTable = table;
    }

    public static HuffmanModel Build(Dictionary<byte, int> frequencies)
    {
        var queue = new PriorityQueue<Node, int>();

        foreach (var (symbol, freq) in frequencies)
            queue.Enqueue(new Node(symbol, freq), freq);

        while (queue.Count > 1)
        {
            var left = queue.Dequeue();
            var right = queue.Dequeue();
            var parent = new Node(null, left.Frequency + right.Frequency, left, right);
            queue.Enqueue(parent, parent.Frequency);
        }

        var root = queue.Dequeue();
        var table = new Dictionary<byte, (uint, int)>();
        BuildEncodingTable(root, 0, 0, table);

        return new HuffmanModel(root, table);
    }

    private static void BuildEncodingTable(Node node, uint code, int length, Dictionary<byte, (uint, int)> table)
    {
        if (node.IsLeaf)
        {
            table[node.Symbol!.Value] = (code, length);
            return;
        }

        BuildEncodingTable(node.Left!, (code << 1) | 0, length + 1, table);
        BuildEncodingTable(node.Right!, (code << 1) | 1, length + 1, table);
    }

    public byte DecodeSymbol(BitReader reader)
    {
        var node = Root;
        while (!node.IsLeaf)
        {
            node = reader.ReadBit() == 0 ? node.Left! : node.Right!;
        }
        return node.Symbol!.Value;
    }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(EncodingTable.Count);
        foreach (var (symbol, (code, length)) in EncodingTable)
        {
            writer.Write(symbol);
            writer.Write(code);
            writer.Write((byte)length);
        }
    }

    public static HuffmanModel Deserialize(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        var table = new Dictionary<byte, (uint, int)>();

        for (int i = 0; i < count; i++)
        {
            byte symbol = reader.ReadByte();
            uint code = reader.ReadUInt32();
            byte length = reader.ReadByte();
            table[symbol] = (code, length);
        }

        var root = RebuildTree(table);
        return new HuffmanModel(root, table);
    }

    private static Node RebuildTree(Dictionary<byte, (uint, int)> table)
    {
        var root = new Node(null, 0);
        foreach (var (symbol, (code, length)) in table)
        {
            var current = root;
            for (int i = length - 1; i >= 0; i--)
            {
                bool bit = ((code >> i) & 1) == 1;
                current = bit
                    ? current.Right ??= new Node(null, 0)
                    : current.Left ??= new Node(null, 0);
            }

            current.Symbol = symbol;
        }

        return root;
    }

    private sealed class Node(byte? symbol, int freq, Node? left = null, Node? right = null)
    {
        public byte? Symbol { get; set; } = symbol;
        public int Frequency { get; } = freq;
        public Node? Left { get; set; } = left;
        public Node? Right { get; set; } = right;

        public bool IsLeaf => Symbol.HasValue;
    }
}
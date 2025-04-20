namespace Itmo.Fitp.Is.Tarch.Core.Algorithms;

public sealed class BurrowsWheelerTransformMoveToFrontHuffmanAlgorithm : IAlgorithm
{
    private const int AlphabetSize = 256;

    public void Encode(Stream input, Stream output)
    {
        using var memoryStream = new MemoryStream();
        input.CopyTo(memoryStream);
        var inputByteCollection = memoryStream.ToArray();

        var (burrowsWheelerTransformedByteCollection, originalStartIndex) =
            BurrowsWheelerTransform(inputByteCollection);
        var moveToFrontEncodedByteCollection = MoveToFrontEncode(burrowsWheelerTransformedByteCollection).ToArray();

        using var inputMemoryStream = new MemoryStream(moveToFrontEncodedByteCollection);
        using var outputMemoryStream = new MemoryStream();
        new HuffmanInternal().Encode(inputMemoryStream, outputMemoryStream);

        using var writer = new BinaryWriter(output);
        writer.Write(originalStartIndex);
        writer.Write(outputMemoryStream.ToArray());
    }

    public void Decode(Stream input, Stream output)
    {
        using var reader = new BinaryReader(input);
        var originalStartIndex = reader.ReadInt32();
        var compressed = reader.ReadBytes((int)(input.Length - input.Position));

        using var memoryStream = new MemoryStream();
        new HuffmanInternal().Decode(new MemoryStream(compressed), memoryStream);
        var mtf = memoryStream.ToArray();

        var bwt = MoveToFrontDecode(mtf);
        var original = InverseBurrowsWheelerTransform(originalStartIndex, bwt.ToArray());

        output.Write(original.ToArray(), 0, original.Count);
    }

    private static (byte[] transformedData, int originalStartIndex) BurrowsWheelerTransform(byte[] input)
    {
        var cyclicShiftTable = new int[input.Length];
        for (var i = 0; i < input.Length; i++)
        {
            cyclicShiftTable[i] = i;
        }

        Array.Sort(cyclicShiftTable, (first, second) =>
        {
            for (var shiftIndex = 0; shiftIndex < input.Length; shiftIndex++)
            {
                var firstChar = input[(first + shiftIndex) % input.Length];
                var secondChar = input[(second + shiftIndex) % input.Length];
                if (firstChar != secondChar)
                {
                    return firstChar - secondChar;
                }
            }

            return 0;
        });

        var result = new byte[input.Length];
        var originalStartIndex = -1;
        for (int i = 0; i < input.Length; i++)
        {
            var shiftedPos = (cyclicShiftTable[i] + input.Length - 1) % input.Length;
            result[i] = input[shiftedPos];
            if (cyclicShiftTable[i] == 0)
            {
                originalStartIndex = i;
            }
        }

        return (result, originalStartIndex);
    }

    private IReadOnlyCollection<byte> InverseBurrowsWheelerTransform(int startIndex, IList<byte> input)
    {
        var size = input.Count;
        var byteSymbolFrequency = new int[AlphabetSize];
        foreach (var byteSymbol in input)
        {
            byteSymbolFrequency[byteSymbol]++;
        }

        var cumulativeByteSymbolFrequency = new int[AlphabetSize];
        for (var index = 1; index < AlphabetSize; index++)
        {
            cumulativeByteSymbolFrequency[index] =
                cumulativeByteSymbolFrequency[index - 1] + byteSymbolFrequency[index - 1];
        }

        var nextPositions = new int[size];
        var usedPositions = new int[AlphabetSize];
        for (var index = 0; index < size; index++)
        {
            var currentByte = input[index];
            nextPositions[cumulativeByteSymbolFrequency[currentByte] + usedPositions[currentByte]++] = index;
        }

        var output = new byte[size];
        var currentPosition = nextPositions[startIndex];
        for (var index = 0; index < size; index++)
        {
            output[index] = input[currentPosition];
            currentPosition = nextPositions[currentPosition];
        }

        return output;
    }

    private IReadOnlyCollection<byte> MoveToFrontEncode(IEnumerable<byte> input)
    {
        var alphabet = Enumerable
            .Range(byte.MinValue, byte.MaxValue + 1)
            .Select(symbol => (byte)symbol)
            .ToList();
        var output = new List<byte>();

        foreach (var byteSymbol in input)
        {
            var symbolIndex = alphabet.IndexOf(byteSymbol);
            output.Add((byte)symbolIndex);
            alphabet.RemoveAt(symbolIndex);
            alphabet.Insert(0, byteSymbol);
        }

        return output.ToArray();
    }

    private IReadOnlyCollection<byte> MoveToFrontDecode(IEnumerable<byte> input)
    {
        var alphabet = Enumerable
            .Range(byte.MinValue, AlphabetSize)
            .Select(symbol => (byte)symbol)
            .ToList();
        var output = new List<byte>();

        foreach (var byteIndex in input)
        {
            var byteSymbol = alphabet[byteIndex];
            output.Add(byteSymbol);
            alphabet.RemoveAt(byteIndex);
            alphabet.Insert(0, byteSymbol);
        }

        return output.ToArray();
    }

    private sealed class HuffmanInternal
    {
        private sealed class Node
        {
            public byte? Value;
            public Node? Left;
            public Node? Right;
            public int Frequency;
            public bool IsLeaf => Left == null && Right == null;
        }

        public void Encode(Stream input, Stream output)
        {
            var freq = new Dictionary<byte, int>();
            int b;
            while ((b = input.ReadByte()) != -1)
            {
                byte bb = (byte)b;
                freq[bb] = freq.TryGetValue(bb, out var f) ? f + 1 : 1;
            }

            var root = BuildTree(freq);
            var codes = BuildCodes(root);

            input.Position = 0;
            using var writer = new BinaryWriter(output);
            WriteTree(writer, root);
            writer.Write((int)input.Length);

            var bw = new BitWriter(writer.BaseStream);
            while ((b = input.ReadByte()) != -1)
            {
                bw.WriteBits(codes[(byte)b]);
            }

            bw.Flush();
        }

        public void Decode(Stream input, Stream output)
        {
            using var reader = new BinaryReader(input);
            var root = ReadTree(reader);
            var length = reader.ReadInt32();

            var br = new BitReader(input);
            var count = 0;
            var node = root;

            while (count < length)
            {
                while (!node.IsLeaf)
                {
                    node = br.ReadBit() ? node.Right! : node.Left!;
                }

                output.WriteByte(node.Value!.Value);
                node = root;
                count++;
            }
        }

        private static Node BuildTree(Dictionary<byte, int> freq)
        {
            var pq = new PriorityQueue<Node, int>();
            foreach (var kv in freq)
            {
                pq.Enqueue(new Node { Value = kv.Key, Frequency = kv.Value }, kv.Value);
            }

            while (pq.Count > 1)
            {
                var l = pq.Dequeue();
                var r = pq.Dequeue();
                var parent = new Node { Left = l, Right = r, Frequency = l.Frequency + r.Frequency };
                pq.Enqueue(parent, parent.Frequency);
            }

            return pq.Dequeue();
        }

        private static Dictionary<byte, List<bool>> BuildCodes(Node root)
        {
            var dict = new Dictionary<byte, List<bool>>();

            void Traverse(Node node, List<bool> path)
            {
                if (node.IsLeaf)
                {
                    dict[node.Value!.Value] = new(path);
                    return;
                }

                path.Add(false);
                Traverse(node.Left!, path);
                path.RemoveAt(path.Count - 1);
                path.Add(true);
                Traverse(node.Right!, path);
                path.RemoveAt(path.Count - 1);
            }

            Traverse(root, []);
            return dict;
        }

        private static void WriteTree(BinaryWriter writer, Node node)
        {
            if (node.IsLeaf)
            {
                writer.Write(true);
                writer.Write(node.Value!.Value);
            }
            else
            {
                writer.Write(false);
                WriteTree(writer, node.Left!);
                WriteTree(writer, node.Right!);
            }
        }

        private static Node ReadTree(BinaryReader reader)
        {
            if (reader.ReadBoolean())
            {
                return new Node { Value = reader.ReadByte() };
            }

            var leftNode = ReadTree(reader);
            var rightNode = ReadTree(reader);
            return new Node { Left = leftNode, Right = rightNode };
        }
    }
}

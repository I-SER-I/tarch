namespace Itmo.Fitp.Is.Tarch.Core;

public sealed class Archiver(IAlgorithm algorithm)
{
    public void Encode(string inputPath, string outputPath)
    {
        using var input = File.OpenRead(inputPath);
        using var output = File.Create(outputPath);

        using var encodedStream = new MemoryStream();
        algorithm.Encode(input, encodedStream);

        var encodedBytes = encodedStream.ToArray();
        output.Write(encodedBytes, 0, encodedBytes.Length);
    }

    public void Decode(string inputPath, string outputPath)
    {
        using var input = File.OpenRead(inputPath);
        using var output = File.Create(outputPath);
        algorithm.Decode(input, output);
    }
}

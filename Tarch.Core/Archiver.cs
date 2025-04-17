namespace Itmo.Fitp.Is.Tarch.Core;

public sealed class Archiver(IAlgorithm algorithm)
{
    public IReadOnlyCollection<CompressResult> EncodeFiles(IEnumerable<string> inputFiles, string archivePath)
    {
        var stats = new List<CompressResult>();

        using var output = File.Create(archivePath);

        foreach (var file in inputFiles)
        {
            var fileName = Path.GetFileName(file);
            var fileNameBytes = Encoding.UTF8.GetBytes(fileName);

            using var input = File.OpenRead(file);
            using var encodedStream = new MemoryStream();
            algorithm.Encode(input, encodedStream);
            var encodedBytes = encodedStream.ToArray();

            output.Write(BitConverter.GetBytes(fileNameBytes.Length));
            output.Write(fileNameBytes);
            output.Write(BitConverter.GetBytes(encodedBytes.Length));
            output.Write(encodedBytes);

            stats.Add(new CompressResult(fileName, input.Length, encodedBytes.Length));
        }
        
        return stats;
    }

    public void DecodeArchive(string archivePath, string outputDirectory)
    {
        using var input = File.OpenRead(archivePath);

        while (input.Position < input.Length)
        {
            var nameLengthBuffer = new byte[4];
            input.Read(nameLengthBuffer);
            int nameLength = BitConverter.ToInt32(nameLengthBuffer);

            var nameBuffer = new byte[nameLength];
            input.Read(nameBuffer);
            var fileName = Encoding.UTF8.GetString(nameBuffer);
            
            var lengthBuffer = new byte[4];
            input.Read(lengthBuffer);
            int compressedLength = BitConverter.ToInt32(lengthBuffer);
            
            using var compressedData = new MemoryStream();
            input.CopyToLimited(compressedData, compressedLength);
            
            compressedData.Position = 0;
            var outputPath = Path.Combine(outputDirectory, fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            using var output = File.Create(outputPath);
            algorithm.Decode(compressedData, output);
        }
    }
}

public record CompressResult(string FileName, long OriginalSize, int EncodedSize);
namespace Itmo.Fitp.Is.Tarch.Core.Algorithms;

public sealed class Dummy : IAlgorithm
{
    public void Encode(Stream input, Stream output) => input.CopyTo(output);

    public void Decode(Stream input, Stream output) => input.CopyTo(output);
}
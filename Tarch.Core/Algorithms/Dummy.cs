namespace Itmo.Fitp.Is.Tarch.Core.Algorithms;

public sealed class DummyAlgorithm : IAlgorithm
{
    public void Encode(Stream input, Stream output) => input.CopyTo(output);

    public void Decode(Stream input, Stream output) => input.CopyTo(output);
}

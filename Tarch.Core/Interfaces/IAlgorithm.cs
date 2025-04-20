namespace Itmo.Fitp.Is.Tarch.Core.Interfaces;

public interface IAlgorithm
{
    void Encode(Stream input, Stream output);
    void Decode(Stream input, Stream output);
}

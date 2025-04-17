namespace Itmo.Fitp.Is.Tarch.Core;

public interface IAlgorithm
{
    void Encode(Stream input, Stream output);
    void Decode(Stream input, Stream output);
}
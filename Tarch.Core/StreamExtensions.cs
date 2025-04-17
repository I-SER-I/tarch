namespace Itmo.Fitp.Is.Tarch.Core;

public static class StreamExtensions
{
    public static void CopyToLimited(this Stream input, Stream output, long length)
    {
        var buffer = new byte[length];
        var remaining = length;
        while (remaining > 0)
        {
            int read = input.Read(buffer, 0, (int) Math.Min(buffer.Length, remaining));
            if (read == 0)
            {
                break;
            }
            output.Write(buffer, 0, read);
            remaining -= read;
        }
    }
}
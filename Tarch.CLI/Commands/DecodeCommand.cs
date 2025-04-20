namespace Itmo.Fitp.Is.Tarch.Cli.Commands;

[UsedImplicitly]
public sealed class DecodeCommand : Command<DecodeSettings>
{
    public override int Execute(CommandContext context, DecodeSettings settings)
    {
        var archiver = new Archiver(new BurrowsWheelerTransformMoveToFrontHuffmanAlgorithm());

        AnsiConsole.MarkupLine("[green]Decoding file[/] " + settings.InputPath);
        archiver.Decode(settings.InputPath, settings.OutputPath);
        AnsiConsole.MarkupLine("[green]Decoded file[/] " + settings.OutputPath);

        return 0;
    }
}

namespace Itmo.Fitp.Is.Tarch.Cli.Commands;

[UsedImplicitly]
public sealed class EncodeCommand : Command<EncodeSettings>
{
    public override int Execute(CommandContext context, EncodeSettings settings)
    {
        var archiver = new Archiver(new BurrowsWheelerTransformMoveToFrontHuffmanAlgorithm());

        AnsiConsole.MarkupLine("[green]Encoding file[/] " + settings.InputPath);
        archiver.Encode(settings.InputPath, settings.OutputPath);
        AnsiConsole.MarkupLine("[green]Encoded file[/] " + settings.OutputPath);

        return 0;
    }
}

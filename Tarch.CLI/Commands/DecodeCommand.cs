namespace Itmo.Fitp.Is.Tarch.Cli.Commands;

[UsedImplicitly]
public sealed class DecodeCommand : Command<DecodeSettings>
{
    public override int Execute(CommandContext context, DecodeSettings settings)
    {
        var archiver = new Archiver(new ContextualHuffman());

        AnsiConsole.MarkupLine("[green]Распауковка нахуй...[/]");
        archiver.DecodeArchive(settings.ArchivePath, settings.OutputDirectory);
        AnsiConsole.MarkupLine("[green]Готово: в папке[/] " + settings.OutputDirectory);

        return 0;
    }
}
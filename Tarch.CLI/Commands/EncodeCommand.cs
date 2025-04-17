namespace Itmo.Fitp.Is.Tarch.Cli.Commands;

[UsedImplicitly]
public sealed class EncodeCommand : Command<EncodeSettings>
{
    public override int Execute(CommandContext context, EncodeSettings settings)
    {
        var archiver = new Archiver(new ContextualHuffman());

        AnsiConsole.MarkupLine("[green]Архивация файлов...[/]");
        var results = archiver.EncodeFiles(settings.Files, settings.ArchivePath);

        var table = new Table
        {
            Border = TableBorder.Rounded,
        };
        table.AddColumn("Файл");
        table.AddColumn("Оригинал");
        table.AddColumn("Сжатый");
        table.AddColumn("Экономия");

        foreach (var result in results)
        {
            var saved = result.OriginalSize - result.EncodedSize;
            var percent = (1 - (result.EncodedSize / (double)result.OriginalSize)) * 100;
            table.AddRow(
                result.FileName,
                $"{result.OriginalSize} B",
                $"{result.EncodedSize} B",
                $"{percent:F1}%"
            );
        }

        AnsiConsole.Write(table);
        return 0;
    }
}
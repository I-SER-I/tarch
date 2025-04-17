namespace Itmo.Fitp.Is.Tarch.Cli.Settings;

[UsedImplicitly]
public sealed class EncodeSettings : CommandSettings
{
    [CommandArgument(0, "<FILES>")]
    [Description("Файлы для архивации")]
    public string[] Files { get; set; } = [];

    [CommandOption("-f|--file <ARCHIVE>")]
    [Description("Имя выходного архива")]
    public string ArchivePath { get; set; } = default!;
}
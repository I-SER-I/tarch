namespace Itmo.Fitp.Is.Tarch.Cli.Settings;

[UsedImplicitly]
public sealed class DecodeSettings : CommandSettings
{
    [CommandArgument(0, "<ARCHIVE>")]
    // [CommandOption("-f|--file <ARCHIVE>")]
    [Description("Путь к архиву")]
    public string ArchivePath { get; set; } = default!;

    [CommandOption("-d|--dir <DIR>")]
    [Description("Папка для распаковки")]
    public string OutputDirectory { get; set; } = default!;
}
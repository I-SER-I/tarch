namespace Itmo.Fitp.Is.Tarch.Cli.Settings;

[UsedImplicitly]
public sealed class DecodeSettings : CommandSettings
{
    [CommandArgument(0, "<input file path>")]
    [Description("Encoded file path")]
    public string InputPath { get; set; } = default!;

    [CommandArgument(1, "<output file path>")]
    [Description("Decoded file path")]
    public string OutputPath { get; set; } = default!;
}

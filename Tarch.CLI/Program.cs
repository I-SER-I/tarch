var app = new CommandApp();

app.Configure(config =>
{
    config
        .SetApplicationName("tarch")
        .AddCommand<EncodeCommand>("encode")
        .WithDescription("Encode file");

    config
        .SetApplicationName("tarch")
        .AddCommand<DecodeCommand>("decode")
        .WithDescription("Decode file");
});

return app.Run(args);

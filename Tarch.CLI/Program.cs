var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("simplearch");
    config.AddCommand<EncodeCommand>("encode").WithDescription("Архивировать файлы");
    config.AddCommand<DecodeCommand>("decode").WithDescription("Распаковать архив");
});

return app.Run(args);
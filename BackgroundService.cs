using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

public class BackgroundService : IHostedService, IDisposable
{
    private int executionCount = 0;
    private readonly ILogger<BackgroundService> _logger;
    private readonly SKDemoConfig _config;
    private Timer? _timer = null;
    private FileSystemWatcher _fsw;
    private readonly IHubContext<PluginHub> _pluginHub;

    public BackgroundService(ILogger<BackgroundService> logger, IOptions<SKDemoConfig> config, IHubContext<PluginHub> pluginHub)
    {
        _logger = logger;
        _config = config.Value;
        _pluginHub = pluginHub;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");
        _fsw = new FileSystemWatcher(_config.PluginFolder, "*.*");
        _fsw.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

        _fsw.Changed += _fileSystemWatcher_Changed;
        _fsw.Created += _fileSystemWatcher_Changed;
        _fsw.Deleted += _fileSystemWatcher_Changed;
        _fsw.IncludeSubdirectories = true;
        _fsw.EnableRaisingEvents = true;
        return Task.CompletedTask;
    }

    private void _fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        _pluginHub.Clients.All.SendAsync("pluginChanged", e.FullPath);
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        _fsw.Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}

public class SKDemoConfig
{
    public string PluginFolder { get; set; } = "G:\\Git\\SKDemo\\Plugins";
}
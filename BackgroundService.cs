using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

public class BackgroundService : IHostedService, IDisposable
{
    private readonly ILogger<BackgroundService> _logger;
    private readonly SKDemoConfig _config;
    private FileSystemWatcher _fsw;
    private readonly IHubContext<PluginHub> _pluginHub;

    public BackgroundService(ILogger<BackgroundService> logger, SKDemoConfig config, IHubContext<PluginHub> pluginHub)
    {
        _logger = logger;
        _config = config;
        _pluginHub = pluginHub;
        _fsw = new FileSystemWatcher(_config.PluginFolder, "*.*");
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");
        _fsw.Changed += _fileSystemWatcher_Changed;
        _fsw.Created += _fileSystemWatcher_Changed;
        _fsw.Deleted += _fileSystemWatcher_Changed;
        _fsw.IncludeSubdirectories = true;
        _fsw.EnableRaisingEvents = true;

        //get the name of all subdirectories in Plugin Folder
        var directories = Directory.GetDirectories(Path.Combine(_config.PluginFolder, "SKDemo")).Select(d => Path.GetFileName(d)).ToArray();
        _pluginHub.Clients.All.SendAsync("PluginsChanged", directories);

        return Task.CompletedTask;
    }

    private void _fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        var directories = Directory.GetDirectories(Path.Combine(_config.PluginFolder, "SKDemo")).Select(d => Path.GetFileName(d)).ToArray();
        _pluginHub.Clients.All.SendAsync("PluginsChanged", directories);
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");
        _fsw.Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _fsw.Dispose();
    }
}

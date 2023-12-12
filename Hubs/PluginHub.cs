using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

public class PluginHub : Hub
{
    private ILogger<PluginHub> _logger;
    private readonly SKDemoConfig _config;

    public PluginHub(ILogger<PluginHub> logger, SKDemoConfig config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task Send(string message)
    {
        //TODO get the plugins from the folder
        await Clients.Caller.SendAsync("Send", message);
    }

    public async Task NewMessage(string message, string prompt, bool usePlugins, bool useMemory)
    {
        //TODO run the AI on the message
        SKGenerator generator = new SKGenerator(_config);
        string reply = await generator.Reply(message, prompt, Clients.Caller, usePlugins, useMemory);
        await Clients.Caller.SendAsync("NewAnswer", reply);
    }

    public async Task NewPlan(string message, string prompt, bool usePlugins, bool useMemory)
    {
        //TODO run the AI on the message
        SKGenerator generator = new SKGenerator(_config);
        
        //send a message after 5 seconds
        Task.Run(async () =>
        {
            await Task.Delay(5000);
            await Clients.Caller.SendAsync("NewPlan", "Planning");
        });

        await generator.Plan(message, prompt, Clients.Caller, usePlugins, useMemory);
    }

    public async Task GetPlugins()
    {
        var directories = Directory.GetDirectories(Path.Combine(_config.PluginFolder, "SKDemo")).Select(d => Path.GetFileName(d)).ToArray();
        await Clients.Caller.SendAsync("PluginsChanged", directories);
    }
}

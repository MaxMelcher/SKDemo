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

    public async Task NewMessage(string message, string prompt)
    {
        //TODO run the AI on the message
        SKGenerator generator = new SKGenerator(_config);
        string reply = await generator.Reply(message, prompt, Clients.Caller);
        await Clients.Caller.SendAsync("NewAnswer", reply);
    }

    public async Task Plan(string message, string prompt)
    {
        //TODO run the AI on the message
        SKGenerator generator = new SKGenerator(_config);
        string reply = await generator.Plan(message, Clients.Caller);
        await Clients.Caller.SendAsync("NewAnswer", reply);
    }
}

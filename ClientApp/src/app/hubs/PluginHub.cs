using Microsoft.AspNetCore.SignalR;

public class PluginHub : Hub
{
    public async Task Send(string message)
    {
        await Clients.All.SendAsync("Send", message);
    }
}

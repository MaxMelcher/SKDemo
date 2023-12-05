using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;

public class SKGenerator
{
    private readonly SKDemoConfig _config;

    public SKGenerator(SKDemoConfig config)
    {
        _config = config;
    }

    public async Task<string> Reply(string message, string prompt, ISingleClientProxy caller)
    {
        var builder = new KernelBuilder();
        builder.WithAzureOpenAIChatCompletion(
         _config.OpenAIModel,                      // Azure OpenAI Deployment Name
         _config.OpenAIModel,                      // Azure OpenAI Deployment Name
         _config.OpenAIEndpoint,                   // Azure OpenAI Endpoint
         _config.OpenAIKey);                       // Azure OpenAI Key

        var kernel = builder.Build();
        var argument = new KernelArguments
        {
            { "input", message }
        };

        var result = await kernel.InvokePromptAsync(prompt, argument);
        return result.GetValue<string>();
    }

    public async Task<string> Plan(string message, ISingleClientProxy caller)
    {
        var builder = new KernelBuilder();
        builder.WithAzureOpenAIChatCompletion(
         _config.OpenAIModel,                      // Azure OpenAI Deployment Name
         _config.OpenAIModel,                      // Azure OpenAI Deployment Name
         _config.OpenAIEndpoint,                   // Azure OpenAI Endpoint
         _config.OpenAIKey);                       // Azure OpenAI Key

        var kernel = builder.Build();

        var prompt = @"{{$input}}";
        await caller.SendAsync("NewPlan", "Generating plan...");

        return "";
    }
}

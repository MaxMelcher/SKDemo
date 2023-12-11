using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Planning;

public class SKGenerator
{
    private readonly SKDemoConfig _config;
    private ILoggerFactory _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().AddDebug());

    public SKGenerator(SKDemoConfig config)
    {
        _config = config;
    }

    public async Task<string> Reply(string input, string prompt, ISingleClientProxy caller)
    {
        var builder = new KernelBuilder();
        builder.AddAzureOpenAIChatCompletion(
         _config.OpenDeployment,                   // Azure OpenAI Deployment Name
         _config.OpenAIModel,                      // Azure OpenAI Endpoint
         _config.OpenAIEndpoint,                   // Azure OpenAI Endpoint
         _config.OpenAIKey);                       // Azure OpenAI Key
        builder.Services.AddSingleton<ILoggerFactory>(_loggerFactory);

        var kernel = builder.Build();
        var function = kernel.CreateFunctionFromPrompt(prompt);

        KernelArguments arguments = new();
        arguments.Add("input", input);

        var result = await function.InvokeAsync(kernel, arguments);
        var answer = result.GetValue<string>();
        return answer!;
    }

    public async Task<string> Plan(string message, string prompt, ISingleClientProxy caller)
    {
        var builder = new KernelBuilder();
        builder.AddAzureOpenAIChatCompletion(
         _config.OpenAIModel,                      // Azure OpenAI Deployment Name
         _config.OpenAIModel,                   // Azure OpenAI Endpoint
         _config.OpenAIEndpoint,                   // Azure OpenAI Endpoint
         _config.OpenAIKey);                       // Azure OpenAI Key

        builder.Services.AddSingleton<ILoggerFactory>(_loggerFactory);

        var kernel = builder.Build();

        await caller.SendAsync("NewPlan", "Generating plan...");

        //load the plugins
        var f = kernel.ImportPluginFromPromptDirectory(_config.PluginFolder, "SKDemo");

        FunctionCallingStepwisePlannerConfig config = new FunctionCallingStepwisePlannerConfig();

        //create a plan based on the prompt and the available functions
        var planner = new FunctionCallingStepwisePlanner(config);

        var promptMessage = prompt + "\n" + message;

        var plannerResult = await planner.ExecuteAsync(kernel, promptMessage);
        var response = plannerResult.FinalAnswer;
        return response!;
    }
}

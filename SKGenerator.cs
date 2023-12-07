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
        builder.WithAzureOpenAIChatCompletionService(
         _config.OpenAIModel,                      // Azure OpenAI Deployment Name
         _config.OpenAIEndpoint,                   // Azure OpenAI Endpoint
         _config.OpenAIKey)                       // Azure OpenAI Key
        .WithLoggerFactory(_loggerFactory);

        var kernel = builder.Build();
        var function = kernel.CreateSemanticFunction(prompt);

        var context = kernel.CreateNewContext();
        context.Variables.Add("input", input);

        var result = await function.InvokeAsync(kernel, context.Variables);
        var answer = result.GetValue<string>();
        return answer!;
    }

    public async Task<string> Plan(string message, string prompt, ISingleClientProxy caller)
    {
        var builder = new KernelBuilder();
        builder.WithAzureOpenAIChatCompletionService(
         _config.OpenAIModel,                      // Azure OpenAI Deployment Name
         _config.OpenAIEndpoint,                   // Azure OpenAI Endpoint
         _config.OpenAIKey)                       // Azure OpenAI Key
        .WithLoggerFactory(_loggerFactory);

        var kernel = builder.Build();
        await caller.SendAsync("NewPlan", "Generating plan...");

        var plannerConfig = new StepwisePlannerConfig
        {

            MaxIterations = 10,
            SemanticMemoryConfig = new()
            {
                RelevancyThreshold = 0.5,
            }
        };

        //load the plugins
        var f = kernel.ImportSemanticFunctionsFromDirectory(_config.PluginFolder, "SKDemo");

        //create a plan based on the prompt and the available functions
        var planner = new StepwisePlanner(kernel, plannerConfig);
        var plan = planner.CreatePlan(prompt);

        var context = kernel.CreateNewContext();
        context.Variables.Add("input", message);
        var kernelResult = await kernel.RunAsync(plan, context.Variables);
        var response = kernelResult.GetValue<string>();
        return response!;
    }
}

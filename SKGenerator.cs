using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.TextEmbedding;
using Microsoft.SemanticKernel.Connectors.Memory.Qdrant;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Plugins.Memory;

public class SKGenerator
{
    private readonly SKDemoConfig _config;
    private ILoggerFactory _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().AddDebug());

    public SKGenerator(SKDemoConfig config)
    {
        _config = config;
    }

    public async Task<string> Reply(string input, string prompt, ISingleClientProxy caller, bool usePlugins, bool useMemory = false)
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

        KernelArguments arguments = new()
        {
            { "input", input }
        };

        if (usePlugins)
        {
            var myPlugins = new MyPlugins();
            kernel.ImportPluginFromObject(myPlugins);
        }

        if (useMemory)
        {
            IMemoryStore store = new QdrantMemoryStore(_config.QdrantEndpoint, 1536, loggerFactory: _loggerFactory);
            SemanticTextMemory textMemory = new(store, GetEmbeddingGenerator());
            var myMemoryPlugin = new MyMemoryPlugin(textMemory);
            kernel.ImportPluginFromObject(myMemoryPlugin);
        }

        var result = await function.InvokeAsync(kernel, arguments);
        var answer = result.GetValue<string>();
        return answer!;
    }

    public async Task<string> Plan(string message, string prompt, ISingleClientProxy caller, bool usePlugins, bool useMemory = false)
    {
        var builder = new KernelBuilder();
        builder.AddAzureOpenAIChatCompletion(
         deploymentName: _config.OpenAIModel,
         modelId: _config.OpenAIModel,
         endpoint: _config.OpenAIEndpoint,
         apiKey: _config.OpenAIKey);

        CustomLogger logger = new CustomLogger("SKGenerator", caller);
        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().AddDebug().AddProvider(new MyLoggerProvider(logger, caller)));
        builder.Services.AddSingleton(_loggerFactory);

        var kernel = builder.Build();

        //attach memory
        var embeddingGenerator = GetEmbeddingGenerator();


        var myPlugins = new MyPlugins();
        kernel.ImportPluginFromObject(myPlugins);

        if (useMemory)
        {
            IMemoryStore store = new QdrantMemoryStore(_config.QdrantEndpoint, 1536, loggerFactory: _loggerFactory);
            SemanticTextMemory textMemory = new(store, embeddingGenerator);
            var myMemoryPlugin = new MyMemoryPlugin(textMemory);
            kernel.ImportPluginFromObject(myMemoryPlugin);
        }

        if (usePlugins)
        {
            //load the plugins
            var f2 = kernel.ImportPluginFromPromptDirectory(Path.Combine(_config.PluginFolder, "SKDemo"));
        }

        FunctionCallingStepwisePlannerConfig config = new FunctionCallingStepwisePlannerConfig();

        //create a plan based on the prompt and the available functions
        var planner = new FunctionCallingStepwisePlanner(config);

        var promptMessage = prompt + "\n" + message;

        var plannerResult = await planner.ExecuteAsync(kernel, promptMessage);
        var response = plannerResult.FinalAnswer;

        await caller.SendAsync("NewAnswer", response);

        return response!;
    }

    private ITextEmbeddingGeneration GetEmbeddingGenerator()
    {
        return new AzureOpenAITextEmbeddingGeneration(
            modelId: _config.OpenAIEmbeddingModel,
            deploymentName: _config.OpenAIEmbeddingModel,
            endpoint: _config.OpenAIEndpoint,
            apiKey: _config.OpenAIKey,
            loggerFactory: _loggerFactory);
    }
}

internal class MyLoggerProvider : ILoggerProvider
{
    private CustomLogger _logger;
    private readonly ISingleClientProxy _caller;

    public MyLoggerProvider(CustomLogger logger, ISingleClientProxy caller)
    {
        _logger = logger;
        _caller = caller;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new CustomLogger(categoryName, _caller);
    }
    public void Dispose()
    {
    }
}
// Custom Logger
public class CustomLogger : ILogger
{
    private readonly string _categoryName;
    private readonly ISingleClientProxy _caller;

    public CustomLogger(string categoryName, ISingleClientProxy caller)
    {
        _categoryName = categoryName;
        _caller = caller;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        // You can implement custom logic for scopes if needed
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        // You can implement custom logic for determining whether the log level is enabled
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        var message = formatter(state, exception);
        _caller.SendAsync("log", $"{DateTime.Now} [{logLevel}] {_categoryName}: {message}");

        if (message.Contains("Function completed"))
        {
            _caller.SendAsync("NewPlan", _categoryName);
        }
    }
}
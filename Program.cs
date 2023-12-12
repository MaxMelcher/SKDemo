using Microsoft.SemanticKernel.Connectors.AI.OpenAI.TextEmbedding;
using Microsoft.SemanticKernel.Connectors.Memory.Qdrant;
using Microsoft.SemanticKernel.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder => builder
        .WithOrigins("https://localhost:44437")
        .AllowAnyMethod()
        .AllowAnyHeader());
});

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

SKDemoConfig config = new SKDemoConfig();
builder.Configuration.GetSection("SKDemoConfig").Bind(config);
builder.Services.AddSingleton(config);

builder.Services.AddHostedService<BackgroundService>();

var embeddingGenerator = new AzureOpenAITextEmbeddingGeneration(
            modelId: config.OpenAIEmbeddingModel, 
            deploymentName: config.OpenAIEmbeddingModel, 
            endpoint: config.OpenAIEndpoint, 
            apiKey: config.OpenAIKey);
IMemoryStore store = new QdrantMemoryStore(config.QdrantEndpoint, 1536);
SemanticTextMemory textMemory = new(store, embeddingGenerator);

builder.Services.AddSingleton<ISemanticTextMemory>(textMemory);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("CorsPolicy");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapHub<PluginHub>("/plugins");
app.MapFallbackToFile("index.html");

app.Run();

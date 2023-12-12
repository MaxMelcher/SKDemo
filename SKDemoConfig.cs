public class SKDemoConfig
{
    public string PluginFolder { get; set; } = "G:\\Git\\SKDemo\\Plugins";
    public string OpenDeployment { get; set; } = "gpt-4";
    public string OpenAIModel { get; set; } = "gpt-4";
    public string OpenAIEmbeddingModel { get; set; } = "text-embedding-ada-002";
    public string OpenAIEndpoint { get; set; } = "https://globalai.openai.azure.com/";
    public string OpenAIKey { get; set; } = "DO-NOT-STEAL-MY-KEY";
    public string QdrantEndpoint { get; set; } = "http://localhost:6333/";
}
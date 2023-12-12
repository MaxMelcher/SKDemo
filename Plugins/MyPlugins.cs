using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Text;
using System.Linq;

public class MyPlugins
{
    private JsonSerializerOptions _options;


    public MyPlugins()
    {
        
    }

    [KernelFunction, Description("Get your current IP address")]
    public async Task<string> GetIPAddress()
    {
        using HttpClient client = new()
        {
            BaseAddress = new Uri("https://api.ipify.org")
        };
        return await client.GetStringAsync("");
    }


}
public class MyMemoryPlugin
{
    private readonly ISemanticTextMemory _memory;
    private JsonSerializerOptions _options;

    public MyMemoryPlugin(ISemanticTextMemory memory)
    {
        _options = new JsonSerializerOptions()
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
        };
        _memory = memory;
    }

    public async Task LoadBlogPosts(SemanticTextMemory memory)
    {
        //get the blog posts from https://melcher.dev/feed/feed.json
        using HttpClient client = new()
        {
            BaseAddress = new Uri("https://melcher.dev")
        };
        var posts = await client.GetFromJsonAsync<List<BlogPost>>("feed/feed.json", _options);

        //add the blog posts to the memory store
        int i = 0;
        foreach (var post in posts)
        {
            var lines = TextChunker.SplitPlainTextLines(post.content, maxTokensPerLine: 2000);
            await memory.SaveInformationAsync("general", lines[0], i.ToString(), post.url, post.title);
            i++;
        }
    }

    [KernelFunction, Description("Get a blog post")]
    public async Task<string> RecallAsync([Description("Get a blog post")] string input,
        ILoggerFactory? loggerFactory,
        CancellationToken cancellationToken = default)
    {
        var log = loggerFactory.CreateLogger("RecallAsync");
        var collection = "general";
        var relevance = 0.7;
        var limit = 1;
        log.LogInformation($"Looking for memories in collection '{collection}' matching '{input}'");

        // Search memory
        var memories = _memory.SearchAsync(collection, input, limit, relevance, true, null, cancellationToken);

        // create a list
        var list = new List<string>();

        // add the memories to the list
        await foreach (var memory in memories)
        {
            list.Add(memory.Metadata.Text);
        }

        if (list.Count == 0)
        {
            log?.LogInformation("Memories not found in collection: {0}", collection);
            return string.Empty;
        }

        log?.LogInformation("Done looking for memories in collection '{0}')", collection);
        return limit == 1 ? list.First() : JsonSerializer.Serialize(list.Select(x => x));
    }
}

public class BlogPost
{
    public string url { get; set; }
    public string title { get; set; }
    public DateTime date_published { get; set; }
    public string description { get; set; }
    public string content { get; set; }
    public List<string> tags { get; set; }
    public List<string> categories { get; set; }
    public string feature { get; set; }
}
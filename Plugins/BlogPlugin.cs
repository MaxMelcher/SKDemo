using System.ComponentModel;
using Microsoft.SemanticKernel;

public class BlogPlugin
{

    [KernelFunction, Description("Get the blog posts")]
    public async Task<List<BlogPost>> GetBlogPosts()
    {
        //get the blog posts from https://melcher.dev/feed/feed.json
        using HttpClient client = new()
            {
                BaseAddress = new Uri("https://melcher.dev")
            };
        var post = await client.GetFromJsonAsync<List<BlogPost>>("feed/feed.json");
        return post;
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
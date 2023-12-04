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
builder.Services.AddHostedService<BackgroundService>();

SKDemoConfig config = new SKDemoConfig();
builder.Configuration.GetSection("SKDemoConfig").Bind(config);
builder.Services.AddSingleton(config);

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

using BlazorWebAssemblySignalRApp.Server.Hubs;
using BlazorWebAssemblySignalRApp.Server.Services;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

builder.Services.AddSingleton<UserResolver>();
builder.Services.AddSingleton<IConnectionsRepository, ConnectionsRepository>();
builder.Services.AddSingleton<QueueListener>();
builder.Services.AddSingleton<ChatHubWrapper>();
builder.Services.AddHostedService(sp => new TimedHostedService(sp.GetRequiredService<QueueListener>().Receive, sp.GetRequiredService<ILogger<TimedHostedService>>()));

var app = builder.Build();

app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();


app.MapRazorPages();
app.MapControllers();
app.MapHub<ChatHub>("/chathub");
app.MapFallbackToFile("index.html");

//app.Services.GetRequiredService<TimedHostedService>().StartAsync(CancellationToken.None);
app.Run();

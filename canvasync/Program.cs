using canvasync.Components;
using canvasync.Containers;
using canvasync.Services;
using Hubs;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddHttpClient();
builder.Services.AddHttpClient("ServerAPI", client =>
{
    // client.BaseAddress = new Uri("https://localhost:5175"); // 서버 주소 고정
    client.BaseAddress = new Uri("https://canvasync.azurewebsites.net"); // 서버 주소 고정
});

builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts =>
{
   opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
       [ "application/octet-stream" ]);
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<StateContainer>();

builder.Services.AddSingleton<PdfService>();

var app = builder.Build();

app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(canvasync.Client._Imports).Assembly);

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.MapControllers();

app.MapHub<CanvasHub>("/canvashub");

app.Run();

using canvasync.Components;
using canvasync.Containers;
using canvasync.Data;
using Hubs;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.AspNetCore.Authentication.Cookies;
using canvasync.Services;
using canvasync.Library.Services;

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
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSignalR()
    .AddMessagePackProtocol();

builder.Services.AddScoped<ICanvasService, CanvasService>();

builder.Services.AddResponseCompression(opts =>
{
   opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
       [ "application/octet-stream" ]);
});


builder.Services.AddSingleton<StateContainer>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "canvasync:";
});
builder.Services.AddSingleton<IDrawingStorageService, RedisDrawingStorageService>();

var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContextFactory<CanvasDbContext>(options => 
    options.UseNpgsql(dataSource));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
       options.Cookie.Name = "auth_token";
       options.LoginPath = "/login";
       options.ExpireTimeSpan = TimeSpan.FromMinutes(60); 
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseResponseCompression();
}

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

// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(canvasync.Client._Imports).Assembly);

app.MapControllers();

app.MapHub<CanvasHub>("/canvashub");

app.Run();

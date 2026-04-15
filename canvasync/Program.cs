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
using StackExchange.Redis;
using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("ServerAPI", client =>
{
    if (builder.Environment.IsDevelopment())
    {
        client.BaseAddress = new Uri("https://localhost:5175"); // 로컬 개발 서버 주소
    }
    else
    {
        client.BaseAddress = new Uri("https://canvasync.azurewebsites.net"); // 배포 서버 주소
    }
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

if (builder.Environment.IsDevelopment())
{
    // 로컬 환경: Redis 캐싱
    var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnectionString));
    builder.Services.AddSingleton<IDrawingStorageService, RedisDrawingStorageService>();
}
else
{
    // 배포 환경: InMemory 캐싱
    builder.Services.AddSingleton<IDrawingStorageService, InMemoryDrawingStorageService>();
}

var azureConnectionString = builder.Configuration.GetConnectionString("AzureStorage");

// Blob 서비스는 실제 사용 시점에 연결 문자열 유효성을 검사합니다.
builder.Services.AddSingleton(_ =>
{
    if (string.IsNullOrWhiteSpace(azureConnectionString))
    {
        throw new InvalidOperationException("ConnectionStrings:AzureStorage is not configured.");
    }

    return new BlobServiceClient(azureConnectionString);
});
builder.Services.AddSingleton<IPdfBlobStorageService, AzureBlobPdfStorageService>();

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

// Keep runtime DB schema in sync to avoid model/column mismatches.
using (var scope = app.Services.CreateScope())
{
    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<CanvasDbContext>>();
    await using var dbContext = await dbContextFactory.CreateDbContextAsync();
    await dbContext.Database.MigrateAsync();
}

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
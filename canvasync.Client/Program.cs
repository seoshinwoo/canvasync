using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using canvasync.Client.Services;
using canvasync.Library.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// api 통신을 위해..
// 서버와 동일한 주소를 기본 주소로 사용하도록 설정..
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddScoped<ICanvasService, CanvasClientService>();
builder.Services.AddScoped<IPdfService, PdfClientService>();

await builder.Build().RunAsync();

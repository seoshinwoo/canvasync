using Microsoft.AspNetCore.SignalR;
using canvasync.Library.Models;

namespace Hubs;

public class CanvasHub : Hub
{
    public async Task SendDrawings(string user, FactorData factorData)
    {
        Console.WriteLine($"Hub 에서 함수 실행!");
        await Clients.Others.SendAsync("ReceiveDrawings", user, factorData);
    }
}
using Microsoft.AspNetCore.SignalR;
using Models;

namespace Hubs;

public class CanvasHub : Hub
{
    public async Task SendDrawings(string user, Factor factor)
    {
        Console.WriteLine($"Hub 에서 함수 실행!");
        await Clients.Others.SendAsync("ReceiveDrawings", user, factor);
    }
}
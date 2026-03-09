using Microsoft.AspNetCore.SignalR;
using canvasync.Library.Models;
using canvasync.Library.Dtos;
using canvasync.Containers;
using System.Collections.Concurrent;
using canvasync.Library.Services;
using canvasync.Services;

namespace Hubs;

public class CanvasHub : Hub
{
    private static ConcurrentDictionary<string, string> ConnectedLectures = new();
    private readonly StateContainer _stateContainer;
    private readonly ICanvasService _canvasService;
    private readonly IDrawingStorageService _drawingStorage;

    public CanvasHub(StateContainer stateContainer, ICanvasService canvasService, IDrawingStorageService drawingStorage)
    {
        _stateContainer = stateContainer;
        _canvasService = canvasService;
        _drawingStorage = drawingStorage;
    }

    public override async Task OnConnectedAsync()
    {
        string connectionId = Context.ConnectionId;

        var httpContext = Context.GetHttpContext();

        var lectureId = httpContext?.Request.Query["lectureId"];
        var memberId = httpContext?.Request.Query["memberId"];

        if (!string.IsNullOrEmpty(lectureId))
        {
            ConnectedLectures.TryAdd(connectionId, lectureId);

            if (!await _drawingStorage.ContainsKeyAsync(lectureId))
            {
                var drawingData = await _canvasService.GetDrawingDataAsync(lectureId, memberId);

                if (drawingData != null)
                {
                    await _drawingStorage.SetAsync(lectureId, drawingData.Drawings);
                }
                else
                {
                    await _drawingStorage.SetAsync(lectureId, new List<List<FactorDto>>());
                }
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string connectionId = Context.ConnectionId;

        if (ConnectedLectures.TryRemove(connectionId, out string removedLectureId))
        {
            if (await _drawingStorage.ContainsKeyAsync(removedLectureId))
            {
                await _drawingStorage.RemoveAsync(removedLectureId);
            }
        }
    }
    
    public async Task SendDrawings(string user, FactorData factorData)
    {
        Console.WriteLine($"Host가 Guest에게 {factorData.FactorDto.FactorType}을 방송");
        await Clients.Others.SendAsync("ReceiveDrawings", user, factorData);

        var pages = await _drawingStorage.GetAsync(factorData.LectureId);

        if (pages is not null)
        {
            while (pages.Count() - 1 < factorData.PageIndex)
            {
                pages.Add(new List<FactorDto>());
            }

            switch (factorData.FactorAction)
            {
                case "Add":
                    pages[factorData.PageIndex].Add(factorData.FactorDto);
                    break;
                case "Delete":
                    pages[factorData.PageIndex].RemoveAt(factorData.FactorIndex);
                    break;
                case "Update":
                case "End":
                    pages[factorData.PageIndex][factorData.FactorIndex] = factorData.FactorDto;
                    break;
            }

            await _drawingStorage.SetAsync(factorData.LectureId, pages);
        }
    }
}
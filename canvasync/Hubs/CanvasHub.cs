using Microsoft.AspNetCore.SignalR;
using canvasync.Library.Models;
using canvasync.Library.Dtos;
using canvasync.Containers;
using System.Collections.Concurrent;
using canvasync.Library.Services;

namespace Hubs;

public class CanvasHub : Hub
{
    // ()
    private static ConcurrentDictionary<string, string> ConnectedLectures = new();
    private readonly StateContainer _stateContainer;
    private readonly ICanvasService _canvasService;

    public CanvasHub(StateContainer stateContainer, ICanvasService canvasService)
    {
        _stateContainer = stateContainer;
        _canvasService = canvasService;
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

            if (!_stateContainer.DrawingStorage.ContainsKey(lectureId))
            {
                var drawingData = await _canvasService.GetDrawingDataAsync(lectureId, memberId);

                if (drawingData != null)
                {
                    _stateContainer.DrawingStorage.Add(lectureId, drawingData.Drawings);
                }
                else
                {
                    _stateContainer.DrawingStorage.Add(lectureId, new List<List<FactorDto>>());
                }
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string connectionId = Context.ConnectionId;

        if (ConnectedLectures.TryRemove(connectionId, out string removedLectureId))
        {
            if (_stateContainer.DrawingStorage.ContainsKey(removedLectureId))
            {
                _stateContainer.DrawingStorage.Remove(removedLectureId);
            }
        }
    }
    
    public async Task SendDrawings(string user, FactorData factorData)
    {
        Console.WriteLine($"Host가 Guest에게 {factorData.FactorDto.FactorType}을 방송");
        await Clients.Others.SendAsync("ReceiveDrawings", user, factorData);

        var pages = _stateContainer.DrawingStorage[factorData.LectureId];

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
                    pages[factorData.PageIndex][factorData.FactorIndex] = factorData.FactorDto;
                    break;
            }
        }

        // var lecture = _stateContainer.Lectures.Where(lec => lec.Id == factorData.LectureId).Select(lec => lec).FirstOrDefault();

        // var factor = FactorDto.FactorDtoToFactor(factorData.FactorDto);

        // switch (factorData.FactorAction)
        // {
        //     case "Add":
        //         lecture.Pages[factorData.PageIndex].Factors.Add(factor);
        //         break;
        //     case "Delete":
        //         lecture.Pages[factorData.PageIndex].Factors.RemoveAt(factorData.FactorIndex);
        //         break;
        //     case "Update":
        //         lecture.Pages[factorData.PageIndex].Factors[factorData.FactorIndex] = factor;
        //         break;
        // }
    }
}
using Microsoft.AspNetCore.SignalR;
using canvasync.Library.Models;
using canvasync.Library.Dtos;
using canvasync.Containers;

namespace Hubs;

public class CanvasHub : Hub
{
    private readonly StateContainer _stateContainer;
    public CanvasHub(StateContainer stateContainer)
    {
        _stateContainer = stateContainer;
    }
    public async Task SendDrawings(string user, FactorData factorData)
    {
        Console.WriteLine($"Hub에 도착한 factorData의 PenPathData.Length : {factorData.FactorDto.PenPathDto.PenPathData.Length}");
        Console.WriteLine($"Hub에 도착한 factorData의 LectureId : {factorData.LectureId }");
        Console.WriteLine($"Hub에 도착한 factorData의 FactorAction : {factorData.FactorAction }");
        await Clients.Others.SendAsync("ReceiveDrawings", user, factorData);

        if (!_stateContainer.LectureDrawings.ContainsKey(factorData.LectureId))
        {
            _stateContainer.LectureDrawings.Add(factorData.LectureId, new List<Page>());
        }

        var factor = FactorDto.FactorDtoToFactor(factorData.FactorDto);

        Console.WriteLine($"_stateContainer.LectureDrawings.Count : {_stateContainer.LectureDrawings[factorData.LectureId]}");
        Console.WriteLine($"factorData.FactorIndex : {factorData.FactorIndex}");

        switch (factorData.FactorAction)
        {
            case "Add":
                _stateContainer.LectureDrawings[factorData.LectureId][factorData.PageIndex].Factors.Add(factor);
                break;
            case "Delete":
                _stateContainer.LectureDrawings[factorData.LectureId][factorData.PageIndex].Factors.RemoveAt(factorData.FactorIndex);
                break;
            case "Update":
                _stateContainer.LectureDrawings[factorData.LectureId][factorData.PageIndex].Factors[factorData.FactorIndex] = factor;
                break;
        }
    }
}
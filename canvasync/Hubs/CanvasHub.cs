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
        // Console.WriteLine($"Hub에 도착한 factorData의 PenPathData.Length : {factorData.FactorDto.PenPathDto.PenPathData.Length}");
        // Console.WriteLine($"Hub에 도착한 factorData의 LectureId : {factorData.LectureId }");
        // Console.WriteLine($"Hub에 도착한 factorData의 FactorAction : {factorData.FactorAction }");
        await Clients.Others.SendAsync("ReceiveDrawings", user, factorData);

        var lecture = _stateContainer.Lectures.Where(lec => lec.Id == factorData.LectureId).Select(lec => lec).FirstOrDefault();

        var factor = FactorDto.FactorDtoToFactor(factorData.FactorDto);

        // Console.WriteLine($"lecture.Pages[0].Factors.Count() : {lecture.Pages[0].Factors.Count()}");
        // Console.WriteLine($"factorData.FactorIndex : {factorData.FactorIndex}");

        switch (factorData.FactorAction)
        {
            case "Add":
                lecture.Pages[factorData.PageIndex].Factors.Add(factor);
                break;
            case "Delete":
                lecture.Pages[factorData.PageIndex].Factors.RemoveAt(factorData.FactorIndex);
                break;
            case "Update":
                lecture.Pages[factorData.PageIndex].Factors[factorData.FactorIndex] = factor;
                break;
        }
    }
}
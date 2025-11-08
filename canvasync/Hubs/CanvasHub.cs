using Microsoft.AspNetCore.SignalR;
using canvasync.Library.Models;
using canvasync.Library.Dtos;
using canvasync.Containers;
using System.Security.Cryptography.X509Certificates;

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
        await Clients.Others.SendAsync("ReceiveDrawings", user, factorData);

        var lecture = _stateContainer.Lectures.Where(lec => lec.Id == factorData.LectureId).Select(lec => lec).FirstOrDefault();

        var factor = FactorDto.FactorDtoToFactor(factorData.FactorDto);

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
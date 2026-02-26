using System.Net.Http.Json;
using canvasync.Client.Pages;
using canvasync.Library.Dtos;
using canvasync.Library.Models;
using canvasync.Library.Services;

namespace canvasync.Client.Services;

public class CanvasClientService : ICanvasService
{
    private readonly HttpClient _httpClient;

    public CanvasClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task AddLectureAsync(Lecture lecture, string memberId)
    {
        // memberId 를 안전하게 보내기 위해서..
        var request = new HttpRequestMessage(HttpMethod.Post, "api/lecture/add-lecture");
        request.Content = JsonContent.Create(lecture);
        request.Headers.Add("X-Member-Id", memberId);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task JoinLectureAsync(string lectureId, string memberId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/lecture/join-lecture/{lectureId}");
        request.Headers.Add("X-Member-Id", memberId);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<Lecture>> GetMyLecturesAsync(string memberId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/lecture/my-lectures");
        request.Headers.Add("X-Member-Id", memberId);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode(); 

        return await response.Content.ReadFromJsonAsync<List<Lecture>>() ?? new List<Lecture>();
    }
    
    public async Task<List<Lecture>> GetJoinedLecturesAsync(string memberId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/lecture/joined-lectures");
        request.Headers.Add("X-Member-Id", memberId);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode(); 

        return await response.Content.ReadFromJsonAsync<List<Lecture>>() ?? new List<Lecture>();
    }

    public async Task<Lecture?> GetLectureAsync(string lectureId)
    {
        return await _httpClient.GetFromJsonAsync<Lecture>($"api/lecture/get-lecture/{lectureId}");
    }

    public async Task<DrawingData?> GetDrawingDataAsync(string lectureId, string memberId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<DrawingData>($"api/drawingdata/get-drawingdata/{lectureId}/{memberId}");
            return response ?? new DrawingData();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
             return new DrawingData();
        }
        catch (System.Text.Json.JsonException) // In case of empty response or null
        {
             return new DrawingData();
        }
    }

    public async Task DeleteLectureAsync(string lectureId)
    {
        var response = await _httpClient.DeleteAsync($"api/lecture/{lectureId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task LeaveLectureAsync(string lectureId, string memberId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/lecture/leave-lecture/{lectureId}");
        request.Headers.Add("X-Member-Id", memberId);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task SaveDrawingDataAsync(DrawingData drawingData)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/lecture/save-drawingdata");
        request.Content = JsonContent.Create(drawingData);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public Task<Lecture?> GetLectureByCodeAsync(string code)
    {
        throw new NotImplementedException();
    }

    public Task<Member> GetMemberAsync(string memberId)
    {
        throw new NotImplementedException();
    }
}
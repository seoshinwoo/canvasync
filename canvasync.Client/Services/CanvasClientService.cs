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
        // 쿠키 인증을 사용하므로 memberId 헤더 불필요 — 서버가 쿠키에서 자동 추출
        var response = await _httpClient.PostAsJsonAsync("api/lecture/add-lecture", lecture);
        response.EnsureSuccessStatusCode();
    }

    public async Task JoinLectureAsync(string lectureId, string memberId)
    {
        var response = await _httpClient.PostAsync($"api/lecture/join-lecture/{lectureId}", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<Lecture>> GetMyLecturesAsync(string memberId)
    {
        return await _httpClient.GetFromJsonAsync<List<Lecture>>("api/lecture/my-lectures")
               ?? new List<Lecture>();
    }
    
    public async Task<List<Lecture>> GetJoinedLecturesAsync(string memberId)
    {
        return await _httpClient.GetFromJsonAsync<List<Lecture>>("api/lecture/joined-lectures")
               ?? new List<Lecture>();
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
        var response = await _httpClient.PostAsync($"api/lecture/leave-lecture/{lectureId}", null);
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

    public Task<Member?> GetMemberAsync(string memberId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CanAccessLectureAsync(string lectureId, string memberId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CanReadDrawingDataAsync(string lectureId, string requestedMemberId, string authenticatedMemberId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsLectureHostAsync(string lectureId, string memberId)
    {
        throw new NotImplementedException();
    }
}

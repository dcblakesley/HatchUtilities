using Hatch.Core.Features.CategoryHierarchy.Models;
using Hatch.Core.Features.HatchUsers.Models;
using Hatch.Core.Features.Projects.RequestAndResponse;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace HatchUtilities;
#pragma warning disable CS8603 // Possible null reference return.

internal class Program
{
    static async Task Main(string[] args)
    {

        var hatchAddress = "https://hatch-api.dev.clarkinc.biz/api";
        var idsApi = "https://idspurchasingapi.dev.clarkinc.biz/categoryhierarchy";

        // copy(window.bearerToken);
        var bearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRhdmlkIEJsYWtlc2xleSIsIndpbmFjY291bnRuYW1lIjoiZGJsYWtlc2xleSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvdXNlcmRhdGEiOiIiLCJlbWFpbCI6ImRibGFrZXNsZXlAd2Vic3RhdXJhbnRzdG9yZS5jb20iLCJuYW1laWQiOiIyMjc3NyIsImZpcnN0TmFtZSI6IkRhdmlkIiwibGFzdE5hbWUiOiJCbGFrZXNsZXkiLCJoYXRjaFVzZXJJZCI6IjE1NCIsInJvbGUiOlsiMzY1LUxpYy1FNSIsIkNsYXJrX0dQUF8xMl9MZW5ndGgiLCJBbnlDb25uZWN0X0RldmVsb3BlciIsIldTUyIsIldTU19zZWMiLCJNaW1lY2FzdC1DeWJlckdyYXBoLVBpbG90IiwiaUxhbmQtQmFja3VwLU9ELUEtRyIsImlMYW5kLUJhY2t1cC1FeC1BbGwiLCJpTGFuZC1CYWNrdXAtT0QtQWxsIiwiaUxhbmQtQmFja3VwLUV4LUEtRyIsIk1pbWVjYXN0LVRyYWluaW5nIiwiTWltZWNhc3QtU3luYy1SZWNvdmVyIiwiV1NTIERldmVsb3BlcnMiLCJMYW5jYXN0ZXIgRGV2ZWxvcG1lbnQiLCJEZXZfV2lyZWxlc3MiLCJJZGxlIFNlc3Npb24gTG9nb3V0IiwiTWFpbHJvb21Vc2VyIiwiTWFpbHJvb21BZG1pbiIsIkxpdGl0eiBCdWlsZGluZyIsIldlYkRldiIsIkRldmVsb3BlcnMiLCJXZWJEZXZlbG9wZXJzIiwiV2ViU3RvcmUiXSwibmJmIjoxNzAyMzM1NzM2LCJleHAiOjE3MDI0MjIxMzYsImlhdCI6MTcwMjMzNTczNn0.uWtdmZL9BNlpgNAVJxIrf0zmrypR2v3TeSVe6Jhvb98";
        
        var client = new HttpClient();
        var serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        client.DefaultRequestHeaders.Authorization = new("Bearer", bearerToken);
        HatchApi.Initialize(client, serializerOptions, hatchAddress);


        //var analytics = new HatchUsageAnalytics(client, serializerOptions);
        //await analytics.Run();

        var chHelper = new CategoryHierarchyMigrationHelper(client, serializerOptions);
        var chFromIds = await CategoryHierarchyMigrationHelper.GetHierarchyFromIds();

    }
}


public static class HatchApi
{
    static HttpClient _client;
    static JsonSerializerOptions? _so;
    static string _address;

    public static void Initialize(HttpClient client, JsonSerializerOptions serializerOptions, string address)
    {
        _client = client;
        _so = serializerOptions;
        _address = address;
    }

    public static async Task<List<HatchUser>> GetUsers()
        => await _client.GetFromJsonAsync<List<HatchUser>>($"{_address}/HatchUsers/GetUsers", _so);

    public static async Task<NormalizedCategoryHierarchy> GetHatchCh()
        => await _client.GetFromJsonAsync<NormalizedCategoryHierarchy>($"{_address}/CategoryHierarchy/GetNormalizedCategoryHierarchy", _so);

    public static async Task<GetProjectsResponse> GetProjects()
        => await _client.GetFromJsonAsync<GetProjectsResponse>($"{_address}/Projects/GetProjectsForProjectList", _so);
}


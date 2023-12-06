using System.Net.Http.Headers;
using System.Text.Json;

namespace HatchUtilities;

internal class Program
{
    static async Task Main(string[] args)
    {
        //Console.WriteLine("Hello, World!");

        // copy(window.bearerToken);
        var bearerToken =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRhdmlkIEJsYWtlc2xleSIsIndpbmFjY291bnRuYW1lIjoiZGJsYWtlc2xleSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvdXNlcmRhdGEiOiJodHRwczovL2Fzc2V0cy53ZWJzdGF1cmFudHN0b3JlLmNvbS9lbXAvODYvNWIvODY1YjM1OWU0NzQwNGMwM2I3YzRjNjI3YmNjMmNjY2MuanBnIiwiZW1haWwiOiJkYmxha2VzbGV5QHdlYnN0YXVyYW50c3RvcmUuY29tIiwibmFtZWlkIjoiMjI3NzciLCJmaXJzdE5hbWUiOiJEYXZpZCIsImxhc3ROYW1lIjoiQmxha2VzbGV5IiwiaGF0Y2hVc2VySWQiOiIzMTkiLCJyb2xlIjpbIjM2NS1MaWMtRTUiLCJDbGFya19HUFBfMTJfTGVuZ3RoIiwiQW55Q29ubmVjdF9EZXZlbG9wZXIiLCJXU1MiLCJXU1Nfc2VjIiwiTWltZWNhc3QtQ3liZXJHcmFwaC1QaWxvdCIsImlMYW5kLUJhY2t1cC1PRC1BLUciLCJpTGFuZC1CYWNrdXAtRXgtQWxsIiwiaUxhbmQtQmFja3VwLU9ELUFsbCIsImlMYW5kLUJhY2t1cC1FeC1BLUciLCJNaW1lY2FzdC1UcmFpbmluZyIsIk1pbWVjYXN0LVN5bmMtUmVjb3ZlciIsIldTUyBEZXZlbG9wZXJzIiwiTGFuY2FzdGVyIERldmVsb3BtZW50IiwiRGV2X1dpcmVsZXNzIiwiSWRsZSBTZXNzaW9uIExvZ291dCIsIk1haWxyb29tVXNlciIsIk1haWxyb29tQWRtaW4iLCJMaXRpdHogQnVpbGRpbmciLCJXZWJEZXYiLCJEZXZlbG9wZXJzIiwiV2ViRGV2ZWxvcGVycyIsIldlYlN0b3JlIl0sIm5iZiI6MTcwMTgwODgyOCwiZXhwIjoxNzAxODk1MjI4LCJpYXQiOjE3MDE4MDg4Mjh9.7iOY6kn3JIyqDaECJZYBRwth1TEsGyauS1upZF-WAhc";
        
        var client = new HttpClient();
        var serializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        Global.Initialize(client, serializerOptions);


        var hatchUsageAnalytics = new HatchUsageAnalytics(client, serializerOptions);
        await hatchUsageAnalytics.Run();

        //var chHelper = new CategoryHierarchyMigrationHelper(client, serializerOptions);
        //await chHelper.CompareHatchVsIdsCh(bearerToken);


    }
}
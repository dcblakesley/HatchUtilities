using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Hatch.Core.Features.AgGridConfigurations.Models;
using Hatch.Core.Features.AgGridFilterConfigurations.Models;
using Hatch.Core.Features.HatchUsers.Models;
using Hatch.Core.Features.Projects.Models;

namespace HatchUtilities;
#pragma warning disable CS8603 // Possible null reference return.

internal class Program
{
    static async Task Main(string[] args)
    {

        var hatchAddress = "https://hatch-api.clarkinc.biz/api";
        //var hatchAddress = "https://localhost:5123/api";
        // copy(window.bearerToken);
        var bearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRhdmlkIEJsYWtlc2xleSIsIndpbmFjY291bnRuYW1lIjoiZGJsYWtlc2xleSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvdXNlcmRhdGEiOiIiLCJlbWFpbCI6ImRibGFrZXNsZXlAd2Vic3RhdXJhbnRzdG9yZS5jb20iLCJuYW1laWQiOiIyMjc3NyIsImZpcnN0TmFtZSI6IkRhdmlkIiwibGFzdE5hbWUiOiJCbGFrZXNsZXkiLCJoYXRjaFVzZXJJZCI6IjE1NCIsInJvbGUiOlsiMzY1LUxpYy1FNSIsIkNsYXJrX0dQUF8xMl9MZW5ndGgiLCJBbnlDb25uZWN0X0RldmVsb3BlciIsIldTUyIsIldTU19zZWMiLCJNaW1lY2FzdC1DeWJlckdyYXBoLVBpbG90IiwiaUxhbmQtQmFja3VwLU9ELUEtRyIsImlMYW5kLUJhY2t1cC1FeC1BbGwiLCJpTGFuZC1CYWNrdXAtT0QtQWxsIiwiaUxhbmQtQmFja3VwLUV4LUEtRyIsIk1pbWVjYXN0LVRyYWluaW5nIiwiTWltZWNhc3QtU3luYy1SZWNvdmVyIiwiV1NTIERldmVsb3BlcnMiLCJMYW5jYXN0ZXIgRGV2ZWxvcG1lbnQiLCJEZXZfV2lyZWxlc3MiLCJJZGxlIFNlc3Npb24gTG9nb3V0IiwiTWFpbHJvb21Vc2VyIiwiTWFpbHJvb21BZG1pbiIsIkxpdGl0eiBCdWlsZGluZyIsIldlYkRldiIsIkRldmVsb3BlcnMiLCJXZWJEZXZlbG9wZXJzIiwiV2ViU3RvcmUiXSwibmJmIjoxNzA2ODI3ODI3LCJleHAiOjE3MDY5MTQyMjcsImlhdCI6MTcwNjgyNzgyN30.Q9wMjBZxx4PysNU6wFiY-R5vllJL-HFxsgyeriMrLMg";
        
        var client = new HttpClient();
        var serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        client.DefaultRequestHeaders.Authorization = new("Bearer", bearerToken);
        HatchApi.Initialize(client, serializerOptions, hatchAddress);
        
        var analytics = new HatchUsageAnalytics(client, serializerOptions);
        await analytics.Run();

        //// Get all responses from the CategoryHierarchyApi
        //var allHatchData = await HatchApi.GetAllHatchData();
        
        //// Save the file
        //var json = JsonSerializer.Serialize(allHatchData, serializerOptions);
        //var now = DateTime.Now;
        //var datestamp = $"{now.Month}-{now.Day} - {now.Hour}-{now.Minute}-{now.Second}";
        //File.WriteAllText($"hatchData -{datestamp}.json", json);

        //await StressTestCh();
        //Console.ReadKey();

        // Get the CH
        var ch = await HatchApi.CategoryHierarchy.GetNormalizedCategoryHierarchy();

        //// Find a Major Category named BAKEWARE
        //var idsCategory = ch.IdsCategories.First(c => c.Id == 587);
 

        //var hatchUsers = await HatchApi.HatchUsers.GetUsers();
        //var director = hatchUsers.FirstOrDefault(x => x.HatchUserId == idsCategory.DirectorId);


    }

    static async Task StressTestCh()
    {
        // Request the CH 100 times
        //Console.ReadKey();
        var sw = new Stopwatch();
        sw.Start();


        for (var i = 0; i < 100; i++)
        {
            var ch = await HatchApi.CategoryHierarchy.GetNormalizedCategoryHierarchy(DateTime.UtcNow.AddDays(1));
            Console.WriteLine(i.ToString());
        }
        Console.WriteLine($"Time: {sw.ElapsedMilliseconds}");
        sw.Stop();
    }


    // Full
    // dev 62224
    //     114132

    // 2 Days
    // 13523
    // 23238

    // 1 Day
    // 12729
    // 17993


}

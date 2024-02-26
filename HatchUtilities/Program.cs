namespace HatchUtilities;

class Program
{
    const ActionToPerform Action = ActionToPerform.GetAllHatchData;
    const TargetEnvironment Environment = TargetEnvironment.Dev;

    static async Task Main(string[] args)
    {
        var hatchAddress = Environment switch
        {
            TargetEnvironment.Production => "https://hatch-api.clarkinc.biz/api",
            TargetEnvironment.Test => "https://hatch-api.test.clarkinc.biz/api",
            TargetEnvironment.Dev => "https://hatch-api.dev.clarkinc.biz/api",
            TargetEnvironment.Local => "https://localhost:5123/api",
        };
       
        // copy(window.bearerToken);
        var bearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRhdmlkIEJsYWtlc2xleSIsIndpbmFjY291bnRuYW1lIjoiZGJsYWtlc2xleSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvdXNlcmRhdGEiOiIiLCJlbWFpbCI6ImRibGFrZXNsZXlAd2Vic3RhdXJhbnRzdG9yZS5jb20iLCJuYW1laWQiOiIyMjc3NyIsImZpcnN0TmFtZSI6IkRhdmlkIiwibGFzdE5hbWUiOiJCbGFrZXNsZXkiLCJoYXRjaFVzZXJJZCI6IjE1NCIsInJvbGUiOlsiMzY1LUxpYy1FNSIsIkNsYXJrX0dQUF8xMl9MZW5ndGgiLCJBbnlDb25uZWN0X0RldmVsb3BlciIsIldTUyIsIldTU19zZWMiLCJNaW1lY2FzdC1DeWJlckdyYXBoLVBpbG90IiwiaUxhbmQtQmFja3VwLU9ELUEtRyIsImlMYW5kLUJhY2t1cC1FeC1BbGwiLCJpTGFuZC1CYWNrdXAtT0QtQWxsIiwiaUxhbmQtQmFja3VwLUV4LUEtRyIsIk1pbWVjYXN0LVRyYWluaW5nIiwiTWltZWNhc3QtU3luYy1SZWNvdmVyIiwiV1NTIERldmVsb3BlcnMiLCJMYW5jYXN0ZXIgRGV2ZWxvcG1lbnQiLCJEZXZfV2lyZWxlc3MiLCJJZGxlIFNlc3Npb24gTG9nb3V0IiwiTWFpbHJvb21Vc2VyIiwiTWFpbHJvb21BZG1pbiIsIkxpdGl0eiBCdWlsZGluZyIsIldlYkRldiIsIkRldmVsb3BlcnMiLCJXZWJEZXZlbG9wZXJzIiwiV2ViU3RvcmUiXSwibmJmIjoxNzA4OTcwMTUzLCJleHAiOjE3MDkwNTY1NTMsImlhdCI6MTcwODk3MDE1M30.E0OgifCVoWNa7k3LO_1_PXEeojcUiZQ_qTLemR6N24o";
        
        var client = new HttpClient();
        var serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        client.DefaultRequestHeaders.Authorization = new("Bearer", bearerToken);
        HatchApi.Initialize(client, serializerOptions, hatchAddress);

        var xyz = await HatchApi.GetAllHatchData();

        
        switch (Action)
        {
            case ActionToPerform.GenerateUsageReport:
                var analytics = new HatchUsageAnalytics(client, serializerOptions);
                await analytics.Run();
                break;
            case ActionToPerform.StressTest:
                await StressTestCh();
                break;
            case ActionToPerform.GetAllHatchData:
                var allHatchData = await HatchApi.GetAllHatchData();
                break;
            default:
                break;
        }
    }

    static async Task StressTestCh()
    {
        // Request the CH 100 times
        //Console.ReadKey();
        var sw = new Stopwatch();
        sw.Start();
        for (var i = 0; i < 100; i++)
        {
            var ch = await HatchApi.CategoryHierarchyApi.GetNormalizedCategoryHierarchy(DateTime.UtcNow.AddDays(1));
            Console.WriteLine(i.ToString());
        }
        Console.WriteLine($"Time: {sw.ElapsedMilliseconds}");
        sw.Stop();
    }
}

public enum ActionToPerform
{
    GenerateUsageReport,
    StressTest,
    GetAllHatchData
}

public enum TargetEnvironment
{
    Production,
    Test,
    Dev,
    Local
}
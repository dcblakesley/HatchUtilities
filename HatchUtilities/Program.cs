namespace HatchUtilities;

class Program
{
    const ActionToPerform Action = ActionToPerform.GenerateUsageReport;
    const TargetEnvironment Environment = TargetEnvironment.Production;

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
        var bearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRhdmlkIEJsYWtlc2xleSIsIndpbmFjY291bnRuYW1lIjoiZGJsYWtlc2xleSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvdXNlcmRhdGEiOiJodHRwczovL2Fzc2V0cy53ZWJzdGF1cmFudHN0b3JlLmNvbS9lbXAvODYvNWIvODY1YjM1OWU0NzQwNGMwM2I3YzRjNjI3YmNjMmNjY2MuanBnIiwiZW1haWwiOiJkYmxha2VzbGV5QHdlYnN0YXVyYW50c3RvcmUuY29tIiwibmFtZWlkIjoiMjI3NzciLCJmaXJzdE5hbWUiOiJEYXZpZCIsImxhc3ROYW1lIjoiQmxha2VzbGV5IiwiaGF0Y2hVc2VySWQiOiIzMTkiLCJyb2xlIjpbIlByb2N1cmVtZW50SHViLVZhdWx0IiwiMzY1LUxpYy1FNSIsIkNsYXJrX0dQUF8xMl9MZW5ndGgiLCJBbnlDb25uZWN0X0RldmVsb3BlciIsIldTUyIsIldTU19zZWMiLCJNaW1lY2FzdC1DeWJlckdyYXBoLVBpbG90IiwiaUxhbmQtQmFja3VwLU9ELUEtRyIsImlMYW5kLUJhY2t1cC1FeC1BbGwiLCJpTGFuZC1CYWNrdXAtT0QtQWxsIiwiaUxhbmQtQmFja3VwLUV4LUEtRyIsIk1pbWVjYXN0LVRyYWluaW5nIiwiTWltZWNhc3QtU3luYy1SZWNvdmVyIiwiV1NTIERldmVsb3BlcnMiLCJMYW5jYXN0ZXIgRGV2ZWxvcG1lbnQiLCJEZXZfV2lyZWxlc3MiLCJJZGxlIFNlc3Npb24gTG9nb3V0IiwiTWFpbHJvb21Vc2VyIiwiTWFpbHJvb21BZG1pbiIsIkxpdGl0eiBCdWlsZGluZyIsIldlYkRldiIsIkRldmVsb3BlcnMiLCJXZWJEZXZlbG9wZXJzIiwiV2ViU3RvcmUiXSwibmJmIjoxNzEwNDQxOTA0LCJleHAiOjE3MTA1MjgzMDQsImlhdCI6MTcxMDQ0MTkwNH0.gFvXhFsofkF8QCOIKYJg2h7XwSAitoVKzrkU1YjVsXI";
        
        var client = new HttpClient();
        var serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        client.DefaultRequestHeaders.Authorization = new("Bearer", bearerToken);
        HatchApi.Initialize(client, serializerOptions, hatchAddress);

        //var a = 0;

        //var projects = await HatchApi.GetProjects();


        //var project = projects.Projects.First(x => x.ProjectId == 26796);
        //var contentSubmissionDate = project.ContentSubmissionDate;
        //var idsSubmissionDate         = project.IdsSubmissionDate;
        //var lastUpdate                      = project.LastUpdated;
    
        //await GetValidationMessages([12037, 12009, 11388, 11899]);

        //return;
        //var xyz = await HatchApi.GetAllHatchData();

        
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

    static async Task GetValidationMessages(IEnumerable<int> ids)
    {        
        var output = new List<string> { $"Project,ItemNumber,ValidationMessages,Barcode" };

        foreach (var id in ids)
        {
            var j = await HatchApi.ContentApi.GetAllItemDetailsForProject(new(id));
            var k = j.ContentItems.ContentExportItems.Where(x => x.HasValidationError).ToList();

            foreach (var z in k)
            {
               // output.Add($"{id},{z.ItemNumber},{z.ValidationMessages?.First()},{z.Barcode}");
            }
        }
        await File.WriteAllLinesAsync("Content Validation 3-6-24.csv", output);
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
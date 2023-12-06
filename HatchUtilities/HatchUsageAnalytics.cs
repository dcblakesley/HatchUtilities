using System.Net.Http.Json;
using System.Text.Json;
using Hatch.Core.Features.HatchUsers.Models;
using Hatch.Core.Infrastructure.Analytics;

namespace HatchUtilities;

/// <summary> Usage - Download the json export of UsageAnaytics from Mongo, drop that file into the bin folder of this project, and run this method. </summary>
public class HatchUsageAnalytics(HttpClient client, JsonSerializerOptions serializerOptions)
{
    
    public async Task Run()
    {
        // Get the text of the file
        var analyticsText = File.ReadAllText("hatch.UserAnalytics.json");

        // Replace the $date with a valid DateTime format
        analyticsText = analyticsText.Replace(" {\n    \"$date\": ", "");
        analyticsText = analyticsText.Replace("\"\n  }\n}", "\"}");

        // Deserialize the file into a list of UserAnalytics
        var analytics = JsonSerializer.Deserialize<List<UserAnalytics>>(analyticsText, serializerOptions);

        var hatchUsers = await client.GetFromJsonAsync<List<HatchUser>>("https://hatch-api.clarkinc.biz/api/HatchUsers/GetUsers", serializerOptions);

        var employeeIds = analytics.Select(x => x.EmployeeId).Distinct().ToList();

        var employeeUsages = new List<EmployeeUsage>();
        foreach (var employeeId in employeeIds)
        {
            var hatchUser = hatchUsers.FirstOrDefault(y => y.EmployeeNumber == employeeId);
            if (hatchUser == null) continue;
            var usages = analytics.Where(x => x.EmployeeId == employeeId).Where(x => x.Timestamp < DateTime.Today).ToList();

            if (usages.Any())
            {
                var hatchLoads = usages.Sum(x => x.HatchLoads);
                var projectLoads = usages.Sum(x => x.ProjectLoads);
                employeeUsages.Add(new(employeeId, hatchUser.EmployeeName, hatchLoads, projectLoads));
            }
        }
        // Write the employee usage to a csv file
        var output = "EmployeeId, Name, Hatch Loads, Project Loads\r\n";
        foreach (var employeeUsage in employeeUsages)
        {
            output += $"{employeeUsage.EmployeeId},{employeeUsage.EmployeeName},{employeeUsage.HatchLoads},{employeeUsage.ProjectLoads}\n";
        }
        // Write the output to a file named EmployeeUsage.csv
        File.WriteAllText("EmployeeUsage.csv", output);

    }
    public record EmployeeUsage(int EmployeeId, string EmployeeName, int HatchLoads, int ProjectLoads);
}
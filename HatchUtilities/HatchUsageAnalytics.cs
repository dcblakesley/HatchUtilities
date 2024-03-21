namespace HatchUtilities;

/// <summary> Usage - Download the json export of UsageAnaytics from Mongo, drop that file into the bin folder of this project, and run this method. </summary>
public class HatchUsageAnalytics(HttpClient client, JsonSerializerOptions serializerOptions)
{
    public async Task Run()
    {
        // Get the text of the file
        var analyticsText = File.ReadAllText("hatch.UserAnalytics.json");
        var builder = new WorkbookBuilder();

        // Replace the $date with a valid DateTime format
        analyticsText = analyticsText.Replace(" {\n    \"$date\": ", "");
        analyticsText = analyticsText.Replace("\"\n  }\n}", "\"}");

        // Deserialize the file into a list of UserAnalytics
        var analytics = JsonSerializer.Deserialize<List<UserAnalytics>>(analyticsText, serializerOptions);
        var hatchUsers = await HatchApi.HatchUsersApi.GetUsers();
        var employeeIds = analytics.Select(x => x.EmployeeId).Distinct().ToList();

        // Generate a list of the starting dates for each month going back to 11/1/2023
        var startMonth = new DateTime(2023, 11, 1);
        var sm = new DateTime(2023, 11, 1);
        var line1 = ",";
        var line2 = "EmployeeId, Name";

        // Create the headers for each month
        while (startMonth < DateTime.Today)
        {
            var endMonth = startMonth.AddMonths(1);
            line1 += $",{startMonth:MMMM yyyy}.,";
            line2 += ",Hatch,Project";
            startMonth = startMonth.AddMonths(1);
        }
        var output = line1 + "\r\n" + line2 + "\r\n";
        
        foreach (var employeeId in employeeIds)
        {
            var hatchUser = hatchUsers.FirstOrDefault(y => y.EmployeeNumber == employeeId);
            if (hatchUser == null) continue;
            var usages = analytics.Where(x => x.EmployeeId == employeeId).Where(x => x.Timestamp < DateTime.Today).ToList();
            startMonth = sm;

            var employeeLine = $"{employeeId},{hatchUser.EmployeeName}";
            if (usages.Any())
            {
                while (startMonth < DateTime.Today)
                {
                    var monthlyUsages = usages.Where(x => x.Timestamp >= startMonth && x.Timestamp < startMonth.AddMonths(1)).ToList();

                    if (monthlyUsages.Any() == false)
                    {
                        employeeLine += ",,";
                        startMonth = startMonth.AddMonths(1);
                        continue;
                    }
                    var hatchLoads = monthlyUsages.Sum(x => x.HatchLoads);
                    var projectLoads = monthlyUsages.Sum(x => x.ProjectLoads);
                    employeeLine += $",{hatchLoads},{projectLoads}";
                    startMonth = startMonth.AddMonths(1);
                }
            }
            output += employeeLine + "\r\n";
        }

        // Write the output to a file named EmployeeUsage.csv
        File.WriteAllText($"Hatch usage report - {DateTime.Today.Month}-{DateTime.Today.Day}-{DateTime.Today.Year}.csv", output);
    }
}
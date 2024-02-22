using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hatch.Core.Features.AgGridConfigurations.Models;
using Hatch.Core.Features.AgGridFilterConfigurations.Models;
using Hatch.Core.Features.AgGridTooltips.Records;
using Hatch.Core.Features.CategoryHierarchy.Models;
using Hatch.Core.Features.CategoryHierarchy.Records;
using Hatch.Core.Features.HatchUsers.Models;
using Hatch.Core.Features.Projects.Models;
using Hatch.Core.Features.Projects.RequestAndResponse;

namespace HatchUtilities;

#pragma warning disable CS8603
public static class HatchApi
{
    public static HttpClient Client { get; set; }
    public static JsonSerializerOptions? So { get; set; }
    public static string Address { get; set;  }

    public static void Initialize(HttpClient client, JsonSerializerOptions serializerOptions, string address)
    {
        Client = client;
        So = serializerOptions;
        Address = address;
    }

    public static async Task<HatchData> GetAllHatchData()
    {
        var hatchData = new HatchData
        {
            CategoryHierarchy =
            {
                GetNormalizedCategoryHierarchy = await CategoryHierarchy.GetNormalizedCategoryHierarchy(),
                GetAllHierarchies = await CategoryHierarchy.GetAllHierarchies(),
                GetHierarchiesAndEmployeeData = await CategoryHierarchy.GetHierarchiesAndEmployeeData()
            }
        };

        // Save the file
        var serializerOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault};

        var json = JsonSerializer.Serialize(hatchData, serializerOptions);
        var now = DateTime.Now;
        var datestamp = $"{now.Month}-{now.Day} - {now.Hour}-{now.Minute}-{now.Second}";
        File.WriteAllText($"hatchData -{datestamp}.json", json);
        return hatchData;
    }

    public static AgGridConfigurationsApi AgGridConfigurations => new();
    public static HatchUsersApi HatchUsers => new();
    public static CategoryHierarchyApi CategoryHierarchy => new();


    public static async Task<NormalizedCategoryHierarchy> GetHatchCh()
        => await Client.GetFromJsonAsync<NormalizedCategoryHierarchy>(
            $"{Address}/CategoryHierarchy/GetNormalizedCategoryHierarchy", So);

    public static async Task<GetProjectsResponse> GetProjects()
        => await Client.GetFromJsonAsync<GetProjectsResponse>($"{Address}/Projects/GetProjectsForProjectList", So);

    // Upsert Project
    public static async Task<HttpResponseMessage> UpsertProject(Project project)
        => await Client.PostAsJsonAsync($"{Address}/Projects/UpsertProject", new UpsertProjectRequest(project), So);


    public class AgGridConfigurationsApi()
    {
        public async Task<AgGridConfiguration> GetAgGridConfigurationById(int id) =>
            await Client.GetFromJsonAsync<AgGridConfiguration>(
                $"{Address}/AgGridConfigurations/GetAgGridConfigurationById/{id}", So);

        public async Task<List<AgGridConfiguration>> GetAgGridConfigurations() =>
            await Client.GetFromJsonAsync<List<AgGridConfiguration>>(
                $"{Address}/AgGridConfigurations/GetAgGridConfigurations", So);

        public async Task<HttpResponseMessage> UpsertAgGridConfiguration(AgGridConfiguration agGridConfiguration) =>
            await Client.PostAsJsonAsync($"{Address}/AgGridConfigurations/UpsertAgGridConfiguration",
                agGridConfiguration, So);
    }

    public class AgGridFilterConfigurationsApi()
    {
    }

    public class AgGridTooltipsApi()
    {
    }

    public class HatchUsersApi()
    {
        public async Task<List<HatchUser>> GetUsers()
            => await Client.GetFromJsonAsync<List<HatchUser>>($"{Address}/HatchUsers/GetUsers", So);
    }

    public class CategoryHierarchyApi()
    {
        public async Task SyncSourceDataAsync()
            => await Client.PostAsync($"{Address}/CategoryHierarchy/SyncSourceData", null);

        public async Task<List<CategoryHierarchyRecord>> GetAllHierarchies()
            => await Client.GetFromJsonAsync<List<CategoryHierarchyRecord>>(
                $"{Address}/CategoryHierarchy/GetAllHierarchies", So);
        
        public async Task<NormalizedCategoryHierarchy> GetNormalizedCategoryHierarchy(DateTime? lastUpdated = null,
            bool useClauthEmployeeId = false)
            => await Client.GetFromJsonAsync<NormalizedCategoryHierarchy>(
                $"{Address}/CategoryHierarchy/GetNormalizedCategoryHierarchy?lastUpdated={lastUpdated}&useClauthEmployeeId={useClauthEmployeeId}",
                So);

        public async Task<HierarchyAndEmployeeRecord> GetHierarchiesAndEmployeeData()
            => await Client.GetFromJsonAsync<HierarchyAndEmployeeRecord>(
                               $"{Address}/CategoryHierarchy/GetHierarchiesAndEmployeeData", So);
    }

}

using Hatch.Core.Features.AgGridConfigurations.Records;
using Hatch.Core.Features.AgGridTooltips.Models;

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
                GetNormalizedCategoryHierarchy = await CategoryHierarchyApi.GetNormalizedCategoryHierarchy(),
                GetAllHierarchies = await CategoryHierarchyApi.GetAllHierarchies(),
                GetHierarchiesAndEmployeeData = await CategoryHierarchyApi.GetHierarchiesAndEmployeeData()
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
    
    public static async Task<NormalizedCategoryHierarchy> GetHatchCh()
        => await Client.GetFromJsonAsync<NormalizedCategoryHierarchy>(
            $"{Address}/CategoryHierarchy/GetNormalizedCategoryHierarchy", So);

    public static async Task<GetProjectsResponse> GetProjects()
        => await Client.GetFromJsonAsync<GetProjectsResponse>($"{Address}/Projects/GetProjectsForProjectList", So);

    // Upsert Project
    public static async Task<HttpResponseMessage> UpsertProject(Project project)
        => await Client.PostAsJsonAsync($"{Address}/Projects/UpsertProject", new UpsertProjectRequest(project), So);


    public static class AgGridConfigurationsApi
    {
        public static async Task<List<AgGridConfiguration>> GetAgGridConfigurations()
        {
            var response = await Client.GetFromJsonAsync<GetAgGridConfigurationsResponse>(
                $"{Address}/AgGridConfigurations/GetAgGridConfigurations", So);
            return response!.Configurations;
        }
        public static async Task<HttpResponseMessage> UpsertAgGridConfiguration(AgGridConfiguration agGridConfiguration)
            => await Client.PostAsJsonAsync($"{Address}/AgGridConfigurations/UpsertAgGridConfiguration", agGridConfiguration, So);
    }
    public static class AgGridFilterConfigurationsApi
    {
        public static async Task<List<AgGridFilterConfiguration>> GetAgGridFilterConfigurations() 
            => await Client.GetFromJsonAsync<List<AgGridFilterConfiguration>>($"{Address}/AgGridFilterConfigurations/GetAgGridFilterConfigurations", So);
       
        public static async Task<HttpResponseMessage> UpsertAgGridFilterConfiguration(AgGridFilterConfiguration agGridFilterConfiguration) 
            => await Client.PostAsJsonAsync($"{Address}/AgGridFilterConfigurations/UpsertAgGridFilterConfiguration", agGridFilterConfiguration, So);
    }
    public static class AgGridTooltipsApi
    {
        public static async Task<TooltipConfigurationList> GetAgGridTooltips()
        {
            var response = (await Client.GetFromJsonAsync<GetAgGridTooltipsResponse>($"{Address}/AgGridTooltips/GetAgGridTooltips", So));
            return response!.Tooltips;
        }
        public static async Task<HttpResponseMessage> UpsertAgGridTooltip(TooltipConfigurationList tooltipConfigurationList) 
            => await Client.PostAsJsonAsync($"{Address}/AgGridTooltips/UpsertAgGridTooltip", tooltipConfigurationList, So);
    }

    public static class HatchUsersApi
    {
        public static async Task<List<HatchUser>> GetUsers()
            => await Client.GetFromJsonAsync<List<HatchUser>>($"{Address}/HatchUsers/GetUsers", So);
    }

    public static class CategoryHierarchyApi
    {
        public static async Task SyncSourceDataAsync()
            => await Client.PostAsync($"{Address}/CategoryHierarchy/SyncSourceData", null);

        public static async Task<List<CategoryHierarchyRecord>> GetAllHierarchies()
            => await Client.GetFromJsonAsync<List<CategoryHierarchyRecord>>(
                $"{Address}/CategoryHierarchy/GetAllHierarchies", So);
        
        public static async Task<NormalizedCategoryHierarchy> GetNormalizedCategoryHierarchy(DateTime? lastUpdated = null,
            bool useClauthEmployeeId = false)
            => await Client.GetFromJsonAsync<NormalizedCategoryHierarchy>(
                $"{Address}/CategoryHierarchy/GetNormalizedCategoryHierarchy?lastUpdated={lastUpdated}&useClauthEmployeeId={useClauthEmployeeId}",
                So);

        public static async Task<HierarchyAndEmployeeRecord> GetHierarchiesAndEmployeeData()
            => await Client.GetFromJsonAsync<HierarchyAndEmployeeRecord>(
                               $"{Address}/CategoryHierarchy/GetHierarchiesAndEmployeeData", So);
    }

}

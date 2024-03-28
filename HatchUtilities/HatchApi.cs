using Hatch.Core.Features.AgGridConfigurations.RequestsAndResponses;
using Hatch.Core.Features.AgGridFilterConfigurations.RequestsAndResponses;
using Hatch.Core.Features.AgGridTooltips.RequestsAndResponses;
using Hatch.Core.Features.CategoryHierarchy.RequestsAndResponses;
using Hatch.Core.Features.Content.RequestsAndResponses;
using Hatch.Core.Features.Projects.RequestsAndResponses;

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
            // 
            AgGridConfigurations = await AgGridConfigurationsApi.GetAgGridConfigurations(),
            AgGridFilterConfigurations = await AgGridFilterConfigurationsApi.GetAgGridFilterConfigurations(),
            AgGridTooltipConfigurations = (await AgGridTooltipsApi.GetAgGridTooltips()).TooltipConfigurations,
            GetNormalizedCategoryHierarchy = await CategoryHierarchyApi.GetNormalizedCategoryHierarchy(),
            GetAllHierarchies = await CategoryHierarchyApi.GetAllHierarchies(),
            //GetHierarchiesAndEmployeeData = await CategoryHierarchyApi.GetHierarchiesAndEmployeeData()
            //Comments = await CommentsApi.GetComments()
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
            => (await Client.GetFromJsonAsync<GetAgGridFilterConfigurationsResponse>($"{Address}/AgGridFilterConfigurations/GetAgGridFilterConfigurations", So))!.Configurations;
       
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
        public static async Task<HttpResponseMessage> UpsertAgGridTooltip(List<TooltipConfiguration> tooltipConfigurations) 
            => await Client.PostAsJsonAsync($"{Address}/AgGridTooltips/UpsertAgGridTooltip", new TooltipConfigurationList {TooltipConfigurations = tooltipConfigurations }, So);
    }
    public static class CategoryHierarchyApi
    {
        public static async Task<List<CategoryHierarchyRecord>> GetAllHierarchies()
            => await Client.GetFromJsonAsync<List<CategoryHierarchyRecord>>($"{Address}/CategoryHierarchy/GetAllHierarchies", So);
        
        public static async Task<NormalizedCategoryHierarchy> GetNormalizedCategoryHierarchy(DateTime? lastUpdated = null, bool useClauthEmployeeId = false)
            => await Client.GetFromJsonAsync<NormalizedCategoryHierarchy>($"{Address}/CategoryHierarchy/GetNormalizedCategoryHierarchy?lastUpdated={lastUpdated}&useClauthEmployeeId={useClauthEmployeeId}", So);

        public static async Task<HierarchyAndEmployeeRecord> GetHierarchiesAndEmployeeData()
            => await Client.GetFromJsonAsync<HierarchyAndEmployeeRecord>($"{Address}/CategoryHierarchy/GetHierarchiesAndEmployeeData", So);
    }
    public static class CommentsApi
    {
        public static async Task<List<Comment>> GetComments(int projectId)
            => await Client.GetFromJsonAsync<List<Comment>>($"{Address}/Comments/GetComments", So);
    }


    public static class HatchUsersApi
    {
        public static async Task<List<HatchUser>> GetUsers()
            => await Client.GetFromJsonAsync<List<HatchUser>>($"{Address}/HatchUsers/GetUsers", So);
    }


    public static class ProjectsApi
    {
        public static async Task<List<Project>> GetProjects()
            => (await Client.GetFromJsonAsync<GetProjectsResponse>($"{Address}/Projects/GetProjectsForProjectList", So))!.Projects.ToList();
            
    }

    public static class ContentApi
    {
        // GetItemsReadyForContent
        public static async Task<GetItemsReadyForContentResponse> GetItemsReadyForContent()
            => await Client.GetFromJsonAsync<GetItemsReadyForContentResponse>($"{Address}/Content/GetItemsReadyForContent", So);

        // GetAllItemDetailsForProject
        public static async Task<GetAllItemDetailsForProjectResponse> GetAllItemDetailsForProject(GetAllItemDetailsForProjectRequest request)
            => await Client.GetFromJsonAsync<GetAllItemDetailsForProjectResponse>($"{Address}/Content/GetAllItemDetailsForProject?projectId={request.ProjectId}", So);

    }

}


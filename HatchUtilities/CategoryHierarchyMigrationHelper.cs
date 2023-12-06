using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Hatch.Core.Features.CategoryHierarchy.Models;
using Hatch.Core.Features.Projects.RequestAndResponse;

namespace HatchUtilities;

public class CategoryHierarchyMigrationHelper(HttpClient client, JsonSerializerOptions serializerOptions)
{
    readonly HttpClient _client = client;
    readonly JsonSerializerOptions _serializerOptions = serializerOptions;

    /// <summary> Generate a spreadsheet showing the differences between the IDS CH and the existing Hatch CH </summary>
    public async Task CompareHatchVsIdsCh(string bearerToken)
    {
        var columnsMajorCategories = new List<List<string>>();
        var columnsIdsCategories = new List<List<string>>();

        // Get a list of all Projects from Hatch
        var getProjectsResponse = await _client.GetFromJsonAsync<GetProjectsResponse>("https://hatch-api.clarkinc.biz/api/Projects/GetProjectsForProjectList", _serializerOptions);

        // Get a list of all MajorCategoryIds from the projects
        var majorCategoryIdsBeingUsedByHatch = getProjectsResponse!.Projects.Select(x => x.MajorCategoryId).Distinct().OrderBy(x => x).ToList();


        var idsChDetailsResponse = await _client.GetFromJsonAsync<IdsGetCategoryDetailsResponse>("https://idspurchasingapi.dev.clarkinc.biz/categoryhierarchy/get-category-details", _serializerOptions);
        var kk = idsChDetailsResponse!.Result;

        // Get Hatch Category Hierarchy
        var hatchCategoryHierarchy = await _client.GetFromJsonAsync<NormalizedCategoryHierarchy>("https://hatch-api.clarkinc.biz/api/CategoryHierarchy/GetNormalizedCategoryHierarchy?useClauthEmployeeId=true", _serializerOptions);

        var allHatchMajorCategoryNames = hatchCategoryHierarchy!.MajorCategories.Select(x => x.Name.Trim()).OrderBy(x => x).ToList();
        allHatchMajorCategoryNames.Insert(0, $"All Hatch Major Categories - {allHatchMajorCategoryNames.Count}");
        columnsMajorCategories.Add(allHatchMajorCategoryNames);

        // Get all the Major Category names from Hatch that are being used by Projects
        var hatchMajorCategoryNames = hatchCategoryHierarchy!.MajorCategories.Where(x=> majorCategoryIdsBeingUsedByHatch.Contains(x.Id)).Select(x => x.Name.Trim()).OrderBy(x => x).ToList();
        hatchMajorCategoryNames.Insert(0, $"Utilized Hatch Major Categories - {hatchMajorCategoryNames.Count}");
        columnsMajorCategories.Add(hatchMajorCategoryNames);

        // Get all the Major Category names from IDS
        var idsMajorCategoryNames = idsChDetailsResponse.Result!.MajorCategories!.Select(x => x.CategoryName.Trim()).OrderBy(x => x).ToList();
        idsMajorCategoryNames.Insert(0, $"All IDS Major Categories - {idsMajorCategoryNames.Count}");
        columnsMajorCategories.Add(idsMajorCategoryNames);

        // Check to see which Major Categories are missing from IDS
        var hatchMajorCategoriesThatDontExistInIds = hatchMajorCategoryNames.Except(idsMajorCategoryNames, StringComparer.OrdinalIgnoreCase).ToList();
        hatchMajorCategoriesThatDontExistInIds.Insert(0, $"Majors that don't exist in IDS - {hatchMajorCategoriesThatDontExistInIds.Count}");
        columnsMajorCategories.Add(hatchMajorCategoriesThatDontExistInIds);


        // IDS Categories

        // Get a list of all IdsCategoryIds from the projects
        var hatchUtilizedIdsCategoryIds = new List<int>();
        foreach (var project in getProjectsResponse.Projects)
        {
            if (project.IdsCategoryIds == null || !project.IdsCategoryIds.Any())
                continue;

            hatchUtilizedIdsCategoryIds.AddRange(project.IdsCategoryIds);
        }
        hatchUtilizedIdsCategoryIds = hatchUtilizedIdsCategoryIds.Distinct().OrderBy(x => x).ToList();
        
        // All Hatch IdsCategoryCodes
        var allHatchIdsCategoryCodes = hatchCategoryHierarchy.IdsCategories.Select(x => x.Code.Trim()).OrderBy(x => x).ToList();
        allHatchIdsCategoryCodes.Insert(0, $"All Hatch IDS Category Codes - {allHatchIdsCategoryCodes.Count}");
        columnsIdsCategories.Add(allHatchIdsCategoryCodes);

        // Utilized Hatch IdsCategoryCodes
        var hatchUtilizedIdsCategoryCodes = hatchCategoryHierarchy.IdsCategories.Where(x => hatchUtilizedIdsCategoryIds.Contains(x.Id)).Select(x => x.Code.Trim()).OrderBy(x => x).ToList();
        hatchUtilizedIdsCategoryCodes.Insert(0, $"Utilized Hatch IDS Category Codes - {hatchUtilizedIdsCategoryCodes.Count}");
        columnsIdsCategories.Add(hatchUtilizedIdsCategoryCodes);

        // All IDS IdsCategoryCodes
        var getIdsCategoriesResponse = await _client.GetFromJsonAsync<IdsGetCategoryHierarchyResponse>("https://idspurchasingapi.dev.clarkinc.biz/categoryhierarchy/get-category-hierarchy", _serializerOptions);
        var idsCategoriesResults = getIdsCategoriesResponse!.Result!.CategoryHierarchies;
        
        // From IDS
        var allIdsCategoryCodesFromIds = idsCategoriesResults!.Select(x => x.ItemCategoryCode.Trim()).OrderBy(x => x).ToList();
        allIdsCategoryCodesFromIds.Insert(0, $"All CategoryCodes in IDS - {allIdsCategoryCodesFromIds.Count}");
        columnsIdsCategories.Add(allIdsCategoryCodesFromIds);

        // Utilized in Hatch, but don't exist in IDS
        var idsCategoryCodesThatDontExistInHatch = hatchUtilizedIdsCategoryCodes.Except(allIdsCategoryCodesFromIds, StringComparer.OrdinalIgnoreCase).ToList();
        idsCategoryCodesThatDontExistInHatch.Insert(0, $"IDS Categories utilized by Hatch, but don't exist in IDS - {idsCategoryCodesThatDontExistInHatch.Count}");
        columnsIdsCategories.Add(idsCategoryCodesThatDontExistInHatch);

        // Export the results to Excel
        var builder = new WorkbookBuilder();
        builder.AddSheet("Major Categories", columnsMajorCategories, true, true);
        builder.AddSheet("IDS Categories", columnsIdsCategories, true, true);
        builder.Save("CH Comparison.xlsx");
    }

    public async Task CreateHatchChFromIdsCh(string bearerToken)
    {
        var idsGetCategoryDetailsResponse = (await _client.GetFromJsonAsync<IdsGetCategoryDetailsResponse>("https://idspurchasingapi.dev.clarkinc.biz/categoryhierarchy/get-category-details", _serializerOptions))?.Result;
        var idsGetCategoriesResponse = (await _client.GetFromJsonAsync<IdsGetCategoryHierarchyResponse>("https://idspurchasingapi.dev.clarkinc.biz/categoryhierarchy/get-category-hierarchy", _serializerOptions))?.Result;



        //var nch = new NormalizedCategoryHierarchy();

        // Get Hatch Category Hierarchy
        //var hatchCategoryHierarchy = await client.GetFromJsonAsync<NormalizedCategoryHierarchy>("https://hatch-api.clarkinc.biz/api/CategoryHierarchy/GetNormalizedCategoryHierarchy?useClauthEmployeeId=true", serializerOptions);

        //var ch = new NormalizedCategoryHierarchy();


    }
}

// 
// public async Task UpdateCategoryHierarchyAsync(CancellationToken cancellationToken)
// => await CategoryHierarchyService.UpdateCategoryHierarchyAsync(cancellationToken);
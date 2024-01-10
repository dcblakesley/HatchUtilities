using System.Net.Http.Json;
using System.Text.Json;
using Hatch.Core.Features.CategoryHierarchy.Models;
using Hatch.Core.Features.CategoryHierarchy.Services;
using Hatch.Core.Features.Projects.Models;
using Hatch.Core.Features.Projects.RequestAndResponse;

namespace HatchUtilities;

public class CategoryHierarchyConversionHelper(HttpClient client, JsonSerializerOptions serializerOptions)
{
    public static async Task<NormalizedCategoryHierarchy> GetHierarchyFromIds()
    {
        // Category Hierarchy (IDS Categories): https://idspurchasingapi.clarkinc.biz/categoryhierarchy/get-category-hierarchy
        // Category Details: https://idspurchasingapi.clarkinc.biz/categoryhierarchy/get-category-details

        var idsAddress = "https://idspurchasingapi.clarkinc.biz/categoryhierarchy";

        var client = new HttpClient();
        var so = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
        Console.WriteLine($"{idsAddress}/get-category-details");
        var idsHierarchy = await client.GetFromJsonAsync<IdsGetCategoryDetailsResponse>($"{idsAddress}/get-category-details", so);
        Console.WriteLine($"{idsAddress}/get-category-hierarchy");
        var idsCategories = await client.GetFromJsonAsync<IdsGetCategoryHierarchyResponse>($"{idsAddress}/get-category-hierarchy", so);

        var ch = idsHierarchy.Result.ToCategoryHierarchy(idsCategories.Result.CategoryHierarchies);

        return ch;
    }

    /// <summary> Generate a spreadsheet showing the differences between the IDS CH and the existing Hatch CH </summary>
    public async Task CompareHatchVsIdsCh()
    {
        var columnsMajorCategories = new List<List<string>>();
        var columnsIdsCategories = new List<List<string>>();

        // Get a list of all Projects from Hatch
        var getProjectsResponse = await HatchApi.GetProjects();

        // Get a list of all MajorCategoryIds from the projects
        var majorCategoryIdsBeingUsedByHatch = getProjectsResponse!.Projects.Select(x => x.MajorCategoryId).Distinct().OrderBy(x => x).ToList();

        // Get 
        var idsCh = await GetHierarchyFromIds();

        // Get Hatch Category Hierarchy
        var hatchCh = await client.GetFromJsonAsync<NormalizedCategoryHierarchy>("https://hatch-api.clarkinc.biz/api/CategoryHierarchy/GetNormalizedCategoryHierarchy?useClauthEmployeeId=true", serializerOptions);

        var allHatchMajorCategoryNames = hatchCh!.MajorCategories.Select(x => x.Name.Trim()).OrderBy(x => x).ToList();
        allHatchMajorCategoryNames.Insert(0, $"All Hatch Major Categories - {allHatchMajorCategoryNames.Count}");
        columnsMajorCategories.Add(allHatchMajorCategoryNames);

        // Get all the Major Category names from Hatch that are being used by Projects
        var hatchMajorCategoryNames = hatchCh!.MajorCategories.Where(x => majorCategoryIdsBeingUsedByHatch.Contains(x.Id)).Select(x => x.Name.Trim()).OrderBy(x => x).ToList();
        hatchMajorCategoryNames.Insert(0, $"Utilized Hatch Major Categories - {hatchMajorCategoryNames.Count}");
        columnsMajorCategories.Add(hatchMajorCategoryNames);

        // Get all the Major Category names from IDS
        var idsMajorCategoryNames = idsCh.MajorCategories.Select(x => x.Name.Trim()).OrderBy(x => x).ToList();
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
        var allHatchIdsCategoryCodes = hatchCh.IdsCategories.Select(x => x.Code.Trim()).OrderBy(x => x).ToList();
        allHatchIdsCategoryCodes.Insert(0, $"All Hatch IDS Category Codes - {allHatchIdsCategoryCodes.Count}");
        columnsIdsCategories.Add(allHatchIdsCategoryCodes);

        // Utilized Hatch IdsCategoryCodes
        var hatchUtilizedIdsCategoryCodes = hatchCh.IdsCategories.Where(x => hatchUtilizedIdsCategoryIds.Contains(x.Id)).Select(x => x.Code.Trim()).OrderBy(x => x).ToList();
        hatchUtilizedIdsCategoryCodes.Insert(0, $"Utilized Hatch IDS Category Codes - {hatchUtilizedIdsCategoryCodes.Count}");
        columnsIdsCategories.Add(hatchUtilizedIdsCategoryCodes);

        // All IDS IdsCategoryCodes
        var getIdsCategoriesResponse = await client.GetFromJsonAsync<IdsGetCategoryHierarchyResponse>("https://idspurchasingapi.clarkinc.biz/categoryhierarchy/get-category-hierarchy", serializerOptions);
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

    public async Task GetDataForConversion()
    {
        // All Projects
        Console.WriteLine("Getting all projects...");
        var allProjects = await HatchApi.GetProjects();

        // Hatch Category Hierarchy
        Console.WriteLine("Getting Hatch Category Hierarchy...");
        var hatchCategoryHierarchy = await HatchApi.GetHatchCh();

        // IDS Category Hierarchy
        Console.WriteLine("Getting IDS Category Hierarchy...");
        var idsCategoryHierarchy = await GetHierarchyFromIds();

        // Create a folder for the data files
        var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Hatch Data");
        if (!Directory.Exists(dataFolder))
            Directory.CreateDirectory(dataFolder);
        
        // Create a file for the Hatch Category Hierarchy
        File.WriteAllText(Path.Combine(dataFolder, "Hatch Category Hierarchy.json"), JsonSerializer.Serialize(hatchCategoryHierarchy));

        // Create a file for the IDS Category Hierarchy
        File.WriteAllText(Path.Combine(dataFolder, "IDS Category Hierarchy.json"), JsonSerializer.Serialize(idsCategoryHierarchy));
        
        // Create a subfolder for projects
        var projectsFolder = Path.Combine(dataFolder, "Projects");
        if (!Directory.Exists(projectsFolder))
            Directory.CreateDirectory(projectsFolder);
        
        // Remove any files in the subfolder
        var projectFiles = Directory.GetFiles(projectsFolder);
        foreach (var projectFile in projectFiles)
            File.Delete(projectFile);

        // Create files for each project
        foreach (var project in allProjects.Projects)
            File.WriteAllText(Path.Combine(projectsFolder, $"{project.ProjectId}.json"), JsonSerializer.Serialize(project));
        
    }

    public async Task PerformCategoryIdReplacements()
    {
        // Get the Data files
        var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Hatch Data");
        var projectsFolder = Path.Combine(dataFolder, "Projects");
        var idsCategoryHierarchy = JsonSerializer.Deserialize<NormalizedCategoryHierarchy>(File.ReadAllText(Path.Combine(dataFolder, "IDS Category Hierarchy.json")));
        var hatchCategoryHierarchy = JsonSerializer.Deserialize<NormalizedCategoryHierarchy>(File.ReadAllText(Path.Combine(dataFolder, "Hatch Category Hierarchy.json")));

        // Major Category Replacements
        var replacementsFile = File.ReadAllText(Path.Combine(dataFolder, "Major Category Replacements.txt"));
        var replacementLines = replacementsFile.Split(Environment.NewLine).ToList();
        var replacements = 
            replacementLines.Select(replacementLine => replacementLine.Split(","))
                .ToDictionary(parts => parts[0], parts => parts[1]);

        // IDS Category Replacements
        var idsCategoryReplacementsFile = File.ReadAllText(Path.Combine(dataFolder, "IDS Category Replacements.txt"));
        var idsCategoryReplacementLines = idsCategoryReplacementsFile.Split(Environment.NewLine).ToList();
        var idsCategoryReplacements = 
            idsCategoryReplacementLines.Select(replacementLine => replacementLine.Split(","))
                .ToDictionary(parts => parts[0], parts => parts[1]);

        // Get all files in the projects folder
        var projectFiles = Directory.GetFiles(projectsFolder);

        // Create a folder to drop completed files
        var completedFolder = Path.Combine(dataFolder, "Completed");
        if (!Directory.Exists(completedFolder))
            Directory.CreateDirectory(completedFolder);
        
        // Loop through each project file
        foreach (var projectFile in projectFiles)
        {
            try
            {
                var project = JsonSerializer.Deserialize<Project>(File.ReadAllText(projectFile));

                // Major Category
                var majorCategoryId = project.MajorCategoryId;
                var allIds = hatchCategoryHierarchy.MajorCategories.Select(x => x.Id).OrderBy(x=>x).ToList();
                var majorCategory = hatchCategoryHierarchy.MajorCategories.FirstOrDefault(x => x.Id == majorCategoryId);
                if (majorCategory == null)
                {
                    Console.WriteLine($"Major Category not found: {majorCategoryId}");
                    continue;
                }
                var majorCategoryName = majorCategory.Name;
                // Check if the Major Category is in the replacements list
                if (replacements.TryGetValue(majorCategoryName, out var replacement))
                    majorCategoryName = replacement;
                
                // Get the matching name from the Ids Category Hierarchy
                var idsMajorCategory = idsCategoryHierarchy.MajorCategories.FirstOrDefault(x => x.Name.Trim().ToLower() == majorCategoryName.Trim().ToLower());
                if (idsMajorCategory == null)
                {
                    Console.WriteLine($"Matching Major Category not found: {majorCategoryName}");
                    continue;
                }
                
                // Set the new Major Category Id
                project.MajorCategoryId = idsMajorCategory.Id;

                // IDS Categories
                var idsCategoryIds = project.IdsCategoryIds;

                // Check if the IDS Categories are in the replacements list
                var newIdsCategoryIds = new List<int>();
                foreach (var idsCategoryId in idsCategoryIds)
                {
                    var idsCategoryCode = hatchCategoryHierarchy.IdsCategories.First(x => x.Id == idsCategoryId).Code;

                    if (idsCategoryReplacements.TryGetValue(idsCategoryCode, out var replacementCode))
                        idsCategoryCode = replacementCode;

                    var idsCategory = idsCategoryHierarchy.IdsCategories.FirstOrDefault(x => x.Code.Trim().ToLower() == idsCategoryCode.Trim().ToLower());
                    if (idsCategory == null)
                    {
                        Console.WriteLine($"IDS Category not found: {idsCategoryCode}");
                        break;
                    }

                    newIdsCategoryIds.Add(idsCategory.Id);
                }

                // Upsert the project
                //var response = await HatchApi.UpsertProject(project);
                //response.EnsureSuccessStatusCode();

                // Move the project file to the completed folder
                var fileName = Path.GetFileName(projectFile);
                File.Move(projectFile, Path.Combine(completedFolder, fileName));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }


    // DTOs only accessible in this class.
    record IdsGetCategoryHierarchyResult(List<IdsCategoryDto>? CategoryHierarchies);
    record IdsGetCategoryHierarchyResponse(IdsGetCategoryHierarchyResult? Result);
    record IdsGetCategoryDetailsResponse(IdsGetCategoryDetailsResult? Result);
    record IdsCategoryDto(
        int ItemCategoryId,
        string ItemCategoryCode,
        string ItemCategoryName,
        int? CommodityId,
        int? BusinessSegmentId,
        int? ProductTypeId,
        int? FamilyId,
        int? MajorCategoryId,
        int? SegmentId,
        int? VicePresidentId,
        int? DirectorId,
        int? SeniorCategoryManagerId,
        int? CategoryManagerId,
        int? CategoryAnalystId,
        int CountOfActiveItems,
        DateTime DateCreated,
        DateTime DateUpdated) {
        public IdsCategory ToIdsCategory()
        {
            var idsCategory = new IdsCategory
            {
                Id = ItemCategoryId,
                Code = ItemCategoryCode,
                Name = ItemCategoryName,

                BusinessSegmentId = BusinessSegmentId ?? 0,
                ProductTypeId = ProductTypeId ?? 0,
                FamilyId = FamilyId ?? 0,
                MajorCategoryId = MajorCategoryId ?? 0,
                SegmentId = SegmentId ?? 0,
                CommodityId = CommodityId ?? 0,

                VicePresidentId = VicePresidentId ?? 0,
                DirectorId = DirectorId ?? 0,
                SeniorCategoryManagerId = SeniorCategoryManagerId,
                CategoryManagerId = CategoryManagerId ?? 0,
                CategoryAnalystId = CategoryAnalystId ?? 0,

                Updated = DateUpdated
            };

            return idsCategory;
        }
    }
    record IdsGetCategoryDetailsResult(
        List<IdsDivisionDto>? BusinessSegments,
        List<IdsDivisionDto>? Commodities,
        List<IdsDivisionDto>? Families,
        List<IdsDivisionDto>? MajorCategories,
        List<IdsDivisionDto>? ProductTypes,
        List<IdsDivisionDto>? Segments) {
        public NormalizedCategoryHierarchy ToCategoryHierarchy(List<IdsCategoryDto> idsCategoryDtos)
            => new()
            {
                BusinessSegments = ConvertIdsToHatchDivisions(BusinessSegments),
                Commodities = ConvertIdsToHatchDivisions(Commodities),
                Families = ConvertIdsToHatchDivisions(Families),
                MajorCategories = ConvertIdsToHatchDivisions(MajorCategories),
                ProductTypes = ConvertIdsToHatchDivisions(ProductTypes),
                Segments = ConvertIdsToHatchDivisions(Segments),
                IdsCategories = ConvertIdsToHatchCategories(idsCategoryDtos)
            };


        List<Division> ConvertIdsToHatchDivisions(List<IdsDivisionDto>? idsDivs) => idsDivs == null ? new() : idsDivs.Select(x => x.ToDivision()).ToList();
        List<IdsCategory> ConvertIdsToHatchCategories(List<IdsCategoryDto> dtos) => dtos.Select(x => x.ToIdsCategory()).ToList();
    }
    record IdsDivisionDto(int CategoryId, string CategoryName, DateTime DateUpdated)
    {
        public Division ToDivision() => new(CategoryId, CategoryName, DateUpdated);
    }
}
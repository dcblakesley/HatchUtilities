namespace HatchUtilities;
/// <summary> Response from https://idspurchasingapi.dev.clarkinc.biz/categoryhierarchy/get-category-hierarchy </summary>
public record IdsGetCategoryHierarchyResponse(IdsGetCategoryHierarchyResult? Result);
public record IdsGetCategoryHierarchyResult(List<IdsCategoryDto>? CategoryHierarchies);

/// <summary> Received from IDS </summary>
public record IdsCategoryDto(
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
    DateTime DateUpdated
);

public record IdsGetCategoryDetailsResult
(
    List<IdsDivisionDto>? BusinessSegments,
    List<IdsDivisionDto>? Commodities,
    List<IdsDivisionDto>? Families,
    List<IdsDivisionDto>? MajorCategories,
    List<IdsDivisionDto>? ProductTypes,
    List<IdsDivisionDto>? Segments
);
public record IdsDivisionDto(int CategoryId, string CategoryName, string DateUpdated);

public record IdsGetCategoryDetailsResponse(IdsGetCategoryDetailsResult? Result);
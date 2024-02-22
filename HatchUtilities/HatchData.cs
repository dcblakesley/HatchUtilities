namespace HatchUtilities;

#pragma warning disable CS8603
public class HatchData
{
    public CategoryHierarchyData CategoryHierarchy { get; set; } = new();

    public class CategoryHierarchyData
    {
        public List<CategoryHierarchyRecord> GetAllHierarchies { get; set; }
        public NormalizedCategoryHierarchy GetNormalizedCategoryHierarchy { get; set; }
        public HierarchyAndEmployeeRecord GetHierarchiesAndEmployeeData { get; set; }
    }
}
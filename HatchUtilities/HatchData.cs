﻿using Hatch.Core.Features.CategoryHierarchy.RequestsAndResponses;

namespace HatchUtilities;

#pragma warning disable CS8603  
#pragma warning disable CS8618

public class HatchData
{
    // AG Grid
    public List<AgGridConfiguration> AgGridConfigurations { get; set; }
    public List<AgGridFilterConfiguration> AgGridFilterConfigurations { get; set; }
    public List<TooltipConfiguration> AgGridTooltipConfigurations { get; set; }

    // CH
    public List<CategoryHierarchyRecord> GetAllHierarchies { get; set; }
    public NormalizedCategoryHierarchy GetNormalizedCategoryHierarchy { get; set; }
    public HierarchyAndEmployeeRecord GetHierarchiesAndEmployeeData { get; set; }

    // Comments
    public List<Comment> Comments { get; set; }

}
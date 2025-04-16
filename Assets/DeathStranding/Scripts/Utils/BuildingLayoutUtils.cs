using UnityEngine;
using System.Collections.Generic;

public static class BuildingLayoutUtils
{
    public static readonly Dictionary<SizeCategory, Vector2Int> SizeDimensions = new Dictionary<SizeCategory, Vector2Int>
    {
        { SizeCategory.Small, new Vector2Int(5, 5) },
        { SizeCategory.Medium, new Vector2Int(7, 7) },
        { SizeCategory.Large, new Vector2Int(9, 9) },
        { SizeCategory.LargeL, new Vector2Int(11, 9) }
    };

    public static bool IsValidPosition(Vector2Int position, SizeCategory size)
    {
        Vector2Int dimensions = SizeDimensions[size];
        return position.x >= 0 && position.x < dimensions.x &&
               position.y >= 0 && position.y < dimensions.y;
    }

    public static bool AreSpecialPointsValid(List<Vector2Int> specialPoints, SizeCategory size)
    {
        if (specialPoints == null || specialPoints.Count == 0)
            return false;

        HashSet<Vector2Int> uniquePoints = new HashSet<Vector2Int>();
        foreach (Vector2Int point in specialPoints)
        {
            if (!IsValidPosition(point, size))
                return false;

            if (!uniquePoints.Add(point))
                return false; // 重复点
        }

        return true;
    }

    public static bool IsLayoutDataValid(string[,] layoutData, SizeCategory size)
    {
        if (layoutData == null)
            return false;

        Vector2Int dimensions = SizeDimensions[size];
        return layoutData.GetLength(0) == dimensions.y && 
               layoutData.GetLength(1) == dimensions.x;
    }

    public static string[,] CreateEmptyLayout(SizeCategory size)
    {
        Vector2Int dimensions = SizeDimensions[size];
        string[,] layout = new string[dimensions.y, dimensions.x];
        
        // 初始化为空地
        for (int y = 0; y < dimensions.y; y++)
        {
            for (int x = 0; x < dimensions.x; x++)
            {
                layout[y, x] = "."; // 空地
            }
        }
        
        return layout;
    }

    public static bool HasRequiredSpecialPoints(List<SpecialPointType> points, BuildingType buildingType)
    {
        HashSet<SpecialPointType> requiredPoints = GetRequiredSpecialPoints(buildingType);
        HashSet<SpecialPointType> providedPoints = new HashSet<SpecialPointType>(points);

        return providedPoints.IsSupersetOf(requiredPoints);
    }

    private static HashSet<SpecialPointType> GetRequiredSpecialPoints(BuildingType buildingType)
    {
        HashSet<SpecialPointType> required = new HashSet<SpecialPointType>();

        switch (buildingType)
        {
            case BuildingType.Yard:
                required.Add(SpecialPointType.DeliveryPoint); // 包裹坞必须有交货点
                break;
            case BuildingType.Hotel:
                required.Add(SpecialPointType.RestPoint); // 旅馆必须有休息点
                break;
            case BuildingType.Bar:
            case BuildingType.Restaurant:
            case BuildingType.Exchange:
                // 这些建筑暂时没有必需的特殊点
                break;
        }

        return required;
    }
} 
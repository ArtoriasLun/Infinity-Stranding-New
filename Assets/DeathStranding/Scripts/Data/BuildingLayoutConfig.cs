using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "BuildingLayoutConfig", menuName = "DeathStranding/Building Layout Config")]
public class BuildingLayoutConfig : ScriptableObject
{
    [Serializable]
    public class BuildingLayout
    {
        public string layoutName;
        public BuildingType buildingType;
        public SizeCategory sizeCategory;
        [TextArea(3, 10)]
        public string layoutData;
        public List<SpecialPointType> requiredPoints = new List<SpecialPointType>();
        public List<SpecialPointType> optionalPoints = new List<SpecialPointType>();

        public char[,] GetLayoutArray()
        {
            string[] rows = layoutData.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            int height = rows.Length;
            int width = rows[0].Length;
            char[,] layout = new char[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    layout[y, x] = rows[y][x];
                }
            }
            return layout;
        }
    }

    public List<BuildingLayout> layouts = new List<BuildingLayout>();

    private void OnValidate()
    {
        foreach (var layout in layouts)
        {
            // Check for empty fields
            if (string.IsNullOrEmpty(layout.layoutName))
            {
                Debug.LogError($"Building layout name cannot be empty");
                continue;
            }

            if (string.IsNullOrEmpty(layout.layoutData))
            {
                Debug.LogError($"Layout data for {layout.layoutName} cannot be empty");
                continue;
            }

            // Validate layout data
            string[] rows = layout.layoutData.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (rows.Length == 0)
            {
                Debug.LogError($"Layout data for {layout.layoutName} is invalid");
                continue;
            }

            // Check if all rows have the same width
            int width = rows[0].Length;
            if (rows.Any(row => row.Length != width))
            {
                Debug.LogError($"Layout {layout.layoutName} has inconsistent row widths");
                continue;
            }

            // Validate size category matches actual dimensions
            int expectedWidth = 0;
            int expectedHeight = 0;
            switch (layout.sizeCategory)
            {
                case SizeCategory.Small:
                    expectedWidth = expectedHeight = 5;
                    break;
                case SizeCategory.Medium:
                    expectedWidth = expectedHeight = 7;
                    break;
                case SizeCategory.Large:
                    expectedWidth = expectedHeight = 9;
                    break;
                case SizeCategory.LargeL:
                    expectedWidth = 11;
                    expectedHeight = 9;
                    break;
            }

            if (width != expectedWidth || rows.Length != expectedHeight)
            {
                Debug.LogError($"Layout {layout.layoutName} dimensions ({width}x{rows.Length}) don't match size category {layout.sizeCategory} ({expectedWidth}x{expectedHeight})");
            }

            // Validate special points
            HashSet<SpecialPointType> uniqueRequired = new HashSet<SpecialPointType>(layout.requiredPoints);
            if (uniqueRequired.Count != layout.requiredPoints.Count)
            {
                Debug.LogWarning($"Layout {layout.layoutName} has duplicate required special points");
            }

            HashSet<SpecialPointType> uniqueOptional = new HashSet<SpecialPointType>(layout.optionalPoints);
            if (uniqueOptional.Count != layout.optionalPoints.Count)
            {
                Debug.LogWarning($"Layout {layout.layoutName} has duplicate optional special points");
            }

            // Check for overlapping required and optional points
            if (uniqueRequired.Overlaps(uniqueOptional))
            {
                Debug.LogError($"Layout {layout.layoutName} has points that are both required and optional");
            }
        }
    }
} 
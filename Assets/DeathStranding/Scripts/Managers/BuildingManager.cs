using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ALUNGAMES
{
    public class BuildingManager : MonoBehaviour
    {
        // 建筑类型定义
        [Serializable]
        public class BuildingType
        {
            public string Name;
            public List<string> RequiredPoints = new List<string>();
            public List<string> OptionalPoints = new List<string>();
            public List<string> RoomSizes = new List<string>();
        }

        // 房间模板定义 - 确保与JS版本完全一致
        private Dictionary<string, string[,]> roomTemplates = new Dictionary<string, string[,]>
        {
            {
                "small", new string[,]
                {
                    {"#", "#", "#", "#"},
                    {"|", ".", ".", "#"},
                    {"#", ".", ".", "|"},
                    {"#", "#", "#", "#"}
                }
            },
            {
                "medium", new string[,]
                {
                    {"#", "#", "#", "#", "#"},
                    {"#", ".", ".", ".", "#"},
                    {"|", ".", ".", ".", "|"},
                    {"#", ".", ".", ".", "#"},
                    {"#", "#", "#", "#", "#"}
                }
            },
            {
                "large", new string[,]
                {
                    {"#", "#", "#", "#", "#", "#"},
                    {"#", ".", ".", ".", ".", "#"},
                    {"#", ".", ".", ".", ".", "|"},
                    {"|", ".", ".", ".", ".", "#"},
                    {"#", ".", ".", ".", ".", "#"},
                    {"#", "#", "#", "#", "#", "#"}
                }
            },
            {
                "largeL", new string[,]
                {
                    {"#", "#", "#", "#", "#", "#"},
                    {"#", ".", ".", ".", ".", "#"},
                    {"|", ".", ".", ".", ".", "|"},
                    {"#", ".", ".", "#", "#", "#"},
                    {"#", ".", ".", "#", ".", "."},
                    {"#", "#", "#", "#", ".", "."}
                }
            }
        };

        // 特殊点类型 - 确保与JS版本完全一致
        private Dictionary<string, string> specialPoints = new Dictionary<string, string>
        {
            {"task", "■"},    // 任务点
            {"delivery", "□"}, // 交付点
            {"rest", "+"}      // 休息点
        };

        // 建筑类型定义
        private Dictionary<string, BuildingType> buildingTypes;

        // 初始化建筑类型
        private void Awake()
        {
            InitializeBuildingTypes();
        }

        private void InitializeBuildingTypes()
        {
            buildingTypes = new Dictionary<string, BuildingType>
            {
                {
                    "bar", new BuildingType
                    {
                        Name = "Bar",
                        RequiredPoints = new List<string> {"task"},
                        OptionalPoints = new List<string> {"rest"},
                        RoomSizes = new List<string> {"small", "medium"}
                    }
                },
                {
                    "yard", new BuildingType
                    {
                        Name = "Yard",
                        RequiredPoints = new List<string> {"delivery"},
                        OptionalPoints = new List<string>(),
                        RoomSizes = new List<string> {"medium", "large", "largeL"}
                    }
                },
                {
                    "hotel", new BuildingType
                    {
                        Name = "Hotel",
                        RequiredPoints = new List<string> {"rest"},
                        OptionalPoints = new List<string>(),
                        RoomSizes = new List<string> {"medium", "large"}
                    }
                },
                {
                    "exchange", new BuildingType
                    {
                        Name = "Exchange",
                        RequiredPoints = new List<string>(),
                        OptionalPoints = new List<string>(),
                        RoomSizes = new List<string> {"small", "medium"}
                    }
                }
            };
        }

        // 生成随机建筑布局
        public Building GenerateBuilding(string type)
        {
            if (!buildingTypes.TryGetValue(type, out BuildingType buildingType))
                return null;

            // 随机选择房间尺寸
            string roomSize = buildingType.RoomSizes[Random.Range(0, buildingType.RoomSizes.Count)];
            string[,] template = roomTemplates[roomSize];

            // 复制模板
            int height = template.GetLength(0);
            int width = template.GetLength(1);
            string[,] layout = new string[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    layout[y, x] = template[y, x];
                }
            }

            Building building = new Building(type, Vector2Int.zero);
            building.Name = buildingType.Name;
            building.Width = width;
            building.Height = height;
            building.Layout = layout;

            // 放置必要的特殊点
            foreach (string point in buildingType.RequiredPoints)
            {
                PlaceSpecialPoint(layout, point);
            }

            // 放置可选特殊点（50%几率）
            foreach (string point in buildingType.OptionalPoints)
            {
                if (Random.value < 0.5f)
                {
                    PlaceSpecialPoint(layout, point);
                }
            }

            return building;
        }

        // 在布局中放置特殊点
        private void PlaceSpecialPoint(string[,] layout, string pointType)
        {
            List<Vector2Int> validPositions = new List<Vector2Int>();

            // 查找所有有效位置（空地板块）
            for (int y = 1; y < layout.GetLength(0) - 1; y++)
            {
                for (int x = 1; x < layout.GetLength(1) - 1; x++)
                {
                    if (layout[y, x] == ".")
                    {
                        validPositions.Add(new Vector2Int(x, y));
                    }
                }
            }

            // 如果找到有效位置，放置特殊点
            if (validPositions.Count > 0)
            {
                Vector2Int pos = validPositions[Random.Range(0, validPositions.Count)];
                layout[pos.y, pos.x] = specialPoints[pointType];
            }
        }

        // 检查建筑中的位置是否是特殊点
        public bool IsSpecialPoint(Building building, int x, int y)
        {
            if (building == null || building.Layout == null) return false;
            if (x < 0 || y < 0 || x >= building.Layout.GetLength(1) || y >= building.Layout.GetLength(0))
                return false;

            string tile = building.Layout[y, x];
            return specialPoints.ContainsValue(tile);
        }

        // 获取特殊点类型
        public string GetSpecialPointType(Building building, int x, int y)
        {
            if (building == null || building.Layout == null) return null;
            if (x < 0 || y < 0 || x >= building.Layout.GetLength(1) || y >= building.Layout.GetLength(0))
                return null;

            string tile = building.Layout[y, x];
            foreach (var pair in specialPoints)
            {
                if (pair.Value == tile)
                    return pair.Key;
            }
            return null;
        }

        // 获取特殊点对应字符
        public string GetSpecialPointChar(string type)
        {
            if (specialPoints.TryGetValue(type, out string value))
                return value;
            return null;
        }
    }
} 
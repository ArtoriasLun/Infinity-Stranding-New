using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ALUNGAMES
{
    public class BuildingManager : MonoBehaviour
    {
        // 建筑类型定义
        [System.Serializable]
        public class BuildingType
        {
            public string Name;
            public List<string> RequiredPoints;  // 必需的特殊点
            public List<string> OptionalPoints;  // 可选的特殊点
            public List<string> RoomSizes;       // 可用的房间大小
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
                PlaceSpecialPoint(building, layout, point);
            }

            // 放置可选特殊点（50%几率）
            foreach (string point in buildingType.OptionalPoints)
            {
                if (Random.value < 0.5f)
                {
                    PlaceSpecialPoint(building, layout, point);
                }
            }

            return building;
        }

        // 在布局中放置特殊点
        private void PlaceSpecialPoint(Building building, string[,] layout, string pointType)
        {
            List<Vector2Int> validPositions = new List<Vector2Int>();
            
            // Find all valid positions (empty floor tiles)
            for (int y = 1; y < layout.GetLength(0) - 1; y++)
            {
                for (int x = 1; x < layout.GetLength(1) - 1; x++)
                {
                    if (layout[y, x] == ".")
                    {
                        // For task points, prefer positions near walls or in corners
                        if (pointType == "task")
                        {
                            bool nearWall = false;
                            // Check adjacent tiles for walls
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                for (int dx = -1; dx <= 1; dx++)
                                {
                                    if (layout[y + dy, x + dx] == "#")
                                    {
                                        nearWall = true;
                                        break;
                                    }
                                }
                                if (nearWall) break;
                            }
                            
                            if (nearWall)
                            {
                                // Higher priority for positions near walls
                                validPositions.Insert(0, new Vector2Int(x, y));
                            }
                            else
                            {
                                validPositions.Add(new Vector2Int(x, y));
                            }
                        }
                        else
                        {
                            validPositions.Add(new Vector2Int(x, y));
                        }
                    }
                }
            }

            // If we found valid positions, place the point
            if (validPositions.Count > 0)
            {
                // For task points, prefer positions from the beginning of the list (near walls)
                Vector2Int pos = validPositions[0];
                string specialPointChar = GetSpecialPointChar(pointType);

                // 获取ASCIIConfig并验证字符
                ASCIIConfig asciiConfig = GameController.Instance.ASCIIRenderer.asciiConfig;
                if (asciiConfig != null && !string.IsNullOrEmpty(specialPointChar))
                {
                    var tileConfigs = asciiConfig.GetTileConfigsByChar(specialPointChar[0],"");
                    if (tileConfigs != null && tileConfigs.Count > 0)
                    {
                        layout[pos.y, pos.x] = specialPointChar;
                        
                        // Store the special point location
                        if (!building.SpecialPoints.ContainsKey(pointType))
                        {
                            building.SpecialPoints[pointType] = new List<Vector2Int>();
                        }
                        building.SpecialPoints[pointType].Add(pos);
                    }
                    else
                    {
                        Debug.LogWarning($"No valid tile configuration found for special point character: {specialPointChar}");
                    }
                }
                else
                {
                    Debug.LogError("ASCIIConfig not found or invalid special point character!");
                }
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
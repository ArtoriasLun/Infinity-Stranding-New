using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ALUNGAMES
{
    public class BuildingManager : MonoBehaviour
    {
        [SerializeField]
        private BuildingLayoutConfig layoutConfig;

        private Dictionary<SpecialPointType, string> specialPointSymbols = new Dictionary<SpecialPointType, string>
        {
            { SpecialPointType.DeliveryPoint, "□" },
            { SpecialPointType.PickupPoint, "■" },
            { SpecialPointType.RestPoint, "+" }
        };

        private void Awake()
        {
            if (layoutConfig == null)
            {
                layoutConfig = Resources.Load<BuildingLayoutConfig>("BuildingLayoutConfig");
                if (layoutConfig == null)
                {
                    Debug.LogError("Failed to load BuildingLayoutConfig from Resources folder!");
                    return;
                }
            }
        }

        // 生成建筑布局
        public Building GenerateBuilding(BuildingType buildingType)
        {
            if (layoutConfig == null) return null;

            // 获取所有匹配的布局
            var matchingLayouts = layoutConfig.layouts.FindAll(l => l.buildingType == buildingType);
            if (matchingLayouts.Count == 0)
            {
                Debug.LogError($"No layouts found for building type: {buildingType}");
                return null;
            }

            // 随机选择一个布局
            var selectedLayout = matchingLayouts[Random.Range(0, matchingLayouts.Count)];

            // 创建建筑实例
            Building building = new Building(buildingType.ToString(), Vector2Int.zero);
            building.Name = buildingType.ToString();

            // 获取布局数组
            char[,] layoutArray = selectedLayout.GetLayoutArray();
            building.Width = layoutArray.GetLength(1);
            building.Height = layoutArray.GetLength(0);

            // 转换为string数组并初始化特殊点
            building.Layout = new string[building.Height, building.Width];
            building.SpecialPoints = new Dictionary<string, List<Vector2Int>>();

            // 处理布局中的每个字符
            for (int y = 0; y < building.Height; y++)
            {
                for (int x = 0; x < building.Width; x++)
                {
                    char c = layoutArray[y, x];
                    string symbol = c.ToString();

                    // 检查是否是特殊点
                    foreach (var pointType in Enum.GetValues(typeof(SpecialPointType)))
                    {
                        var specialPointType = (SpecialPointType)pointType;
                        if (c.ToString().Equals(GetSpecialPointSymbol(specialPointType)))
                        {
                            // 存储特殊点位置
                            string pointTypeStr = specialPointType.ToString().ToLower();
                            if (!building.SpecialPoints.ContainsKey(pointTypeStr))
                            {
                                building.SpecialPoints[pointTypeStr] = new List<Vector2Int>();
                            }
                            building.SpecialPoints[pointTypeStr].Add(new Vector2Int(x, y));
                        }
                    }

                    building.Layout[y, x] = symbol;
                }
            }

            // 随机添加可选特殊点（50%几率）
            foreach (var optionalPoint in selectedLayout.optionalPoints)
            {
                if (Random.value < 0.5f)
                {
                    TryPlaceOptionalPoint(building, optionalPoint);
                }
            }

            return building;
        }

        private void TryPlaceOptionalPoint(Building building, SpecialPointType pointType)
        {
            List<Vector2Int> validPositions = new List<Vector2Int>();
            
            // 寻找所有有效位置（空地）
            for (int y = 1; y < building.Height - 1; y++)
            {
                for (int x = 1; x < building.Width - 1; x++)
                {
                    if (building.Layout[y, x] == ".")
                        {
                            validPositions.Add(new Vector2Int(x, y));
                    }
                }
            }

            if (validPositions.Count > 0)
            {
                Vector2Int pos = validPositions[Random.Range(0, validPositions.Count)];
                string symbol = GetSpecialPointSymbol(pointType);

                // 获取ASCIIConfig并验证字符
                ASCIIConfig asciiConfig = GameController.Instance.ASCIIRenderer.asciiConfig;
                if (asciiConfig != null && !string.IsNullOrEmpty(symbol))
                {
                    var tileConfigs = asciiConfig.GetTileConfigsByChar(symbol[0], "");
                    if (tileConfigs != null && tileConfigs.Count > 0)
                    {
                        building.Layout[pos.y, pos.x] = symbol;

                        string pointTypeStr = pointType.ToString().ToLower();
                        if (!building.SpecialPoints.ContainsKey(pointTypeStr))
                        {
                            building.SpecialPoints[pointTypeStr] = new List<Vector2Int>();
                        }
                        building.SpecialPoints[pointTypeStr].Add(pos);
                    }
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
            return specialPointSymbols.ContainsValue(tile);
        }

        // 获取特殊点类型
        public string GetSpecialPointType(Building building, int x, int y)
        {
            if (building == null || building.Layout == null) return null;
            if (x < 0 || y < 0 || x >= building.Layout.GetLength(1) || y >= building.Layout.GetLength(0))
                return null;

            string tile = building.Layout[y, x];
            foreach (var pair in specialPointSymbols)
            {
                if (pair.Value == tile)
                    return pair.Key.ToString().ToLower();
            }
            return null;
        }

        // 获取特殊点对应字符
        private string GetSpecialPointSymbol(SpecialPointType pointType)
        {
            if (specialPointSymbols.TryGetValue(pointType, out string symbol))
                return symbol;
            return null;
        }
    }
} 
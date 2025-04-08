using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ALUNGAMES
{
    public class CityManager : MonoBehaviour
    {
        private List<City> cities = new List<City>();
        private Dictionary<string, string> citySymbols = new Dictionary<string, string>();
        private readonly string[] citySymbolPool = { "α", "β", "γ", "δ", "ε", "ζ", "η", "θ", "ι", "κ" };
        
        // 城市生成参数
        private const int MIN_CITY_SIZE = 20;
        private const int MAX_CITY_SIZE = 30;
        private const float CITY_SPACING = 40f;

        // 多种城市布局模板
        private readonly List<string[,]> cityLayouts = new List<string[,]>();

        private void InitializeCityLayouts()
        {
            // 标准方形布局
            string[,] standardLayout = new string[,] {
                {".", ".", ".", ".", ".", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "|", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "|"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#"}
            };

            // L形布局
            string[,] lShapedLayout = new string[,] {
                {".", ".", ".", ".", ".", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {".", ".", ".", ".", ".", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", ".", ".", ".", "."},
                {".", ".", ".", ".", ".", "|", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", ".", ".", ".", "."},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", ".", ".", ".", "."},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", ".", ".", ".", "."},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", ".", ".", ".", "."},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "|", ".", ".", ".", "."},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", ".", ".", ".", "."},
                {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", ".", ".", ".", "."},
                {".", ".", ".", ".", ".", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", ".", ".", ".", "."}
            };

            // 圆形布局
            string[,] circularLayout = new string[,] {
                {".", ".", ".", ".", ".", ".", ".", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", ".", ".", ".", ".", ".", "."},
                {".", ".", ".", ".", ".", "#", "#", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", "#", "#", ".", ".", ".", "."},
                {".", ".", ".", ".", "#", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", "#", ".", ".", "."},
                {".", ".", ".", "#", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", "#", ".", "."},
                {".", ".", "#", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", "#", "."},
                {".", "#", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", "#"},
                {".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {"#", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {"#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {"#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {"#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {"#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {"#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {"#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {"#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {"#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#"},
                {"#", "|", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "|"},
                {"#", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", "#"},
                {".", "#", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", "#", "."},
                {".", ".", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", ".", "."}
            };

            cityLayouts.Add(standardLayout);
            cityLayouts.Add(lShapedLayout);
            cityLayouts.Add(circularLayout);
        }

        // 在构造函数或Awake中初始化布局
        private void Awake()
        {
            InitializeCityLayouts();
        }

        // 初始化城市生成
        public void InitCities(int count, int worldWidth, int worldHeight)
        {
            GenerateCities(count, worldWidth, worldHeight);
        }

        // 生成城市
        private void GenerateCities(int count, int worldWidth, int worldHeight)
        {
            cities.Clear();
            citySymbols.Clear();
            HashSet<string> usedPositions = new HashSet<string>();

            // 确保城市数量在配置范围内
            DeathStrandingConfig gameConfig = GameController.Instance.GameConfig;
            count = Mathf.Clamp(count, gameConfig.minCityCount, gameConfig.maxCityCount);

            // 首先在左上角 (0,0) 生成第一个城市（α城市）
            string firstCityPosKey = "0,0";
            usedPositions.Add(firstCityPosKey);
            
            City firstCity = new City(
                GetRandomCityName(),
                new Vector2Int(0, 0),
                40  // 使用较大的尺寸确保覆盖玩家初始位置
            );
            
            firstCity.Buildings = GenerateCityBuildings(40);  // 生成更大的建筑布局
            firstCity.Symbol = 'α';  // 使用α作为第一个城市的符号
            citySymbols[firstCityPosKey] = "α";
            cities.Add(firstCity);
            
            Debug.Log($"在 (0,0) 生成了α城市: {firstCity.Name}，尺寸: 40x40");

            while (cities.Count < count)
            {
                int x = Random.Range(0, worldWidth);
                int y = Random.Range(0, worldHeight);
                string posKey = $"{x},{y}";

                if (!usedPositions.Contains(posKey))
                {
                    usedPositions.Add(posKey);
                    
                    // 使用固定尺寸20，与JS版本一致
                    City city = new City(
                        GetRandomCityName(),
                        new Vector2Int(x, y),
                        20 // 固定城市大小为20
                    );

                    city.Buildings = GenerateCityBuildings(20);  // 为其他城市使用标准尺寸20
                    
                    // 使用希腊字母池中的符号
                    int symbolIndex = 0;
                    if (citySymbolPool.Length > 0)
                    {
                        symbolIndex = Mathf.Min(cities.Count, citySymbolPool.Length - 1);
                        if (symbolIndex < 0) symbolIndex = 0; // 防止负索引
                    }
                    
                    // 安全检查
                    if (citySymbolPool.Length > 0)
                    {
                        string symbol = citySymbolPool[symbolIndex]; 
                        citySymbols[posKey] = symbol;
                        
                        // 设置城市的符号
                        if (!string.IsNullOrEmpty(symbol) && symbol.Length > 0)
                        {
                            city.Symbol = symbol[0];
                        }
                    }
                    else
                    {
                        // 如果符号池为空，使用默认符号
                        citySymbols[posKey] = "C";
                        city.Symbol = 'C';
                    }
                    
                    cities.Add(city);
                }
            }
        }

        // 生成随机城市名称
        private string GetRandomCityName()
        {
            string[] prefixes = { "New", "Old", "East", "West", "North", "South" };
            string[] names = { "Haven", "Ridge", "Port", "Creek", "Summit", "Valley" };
            
            bool usePrefix = Random.value > 0.5f;
            string prefix = usePrefix ? prefixes[Random.Range(0, prefixes.Length)] + " " : "";
            string name = names[Random.Range(0, names.Length)];
            
            return prefix + name;
        }

        // 为城市生成建筑
        private List<Building> GenerateCityBuildings(int size)
        {
            List<Building> buildings = new List<Building>();
            
            // 通过GameController.Instance获取BuildingManager
            BuildingManager buildingManager = GameController.Instance.BuildingManager;
            if (buildingManager == null)
            {
                Debug.LogError("BuildingManager not found!");
                return buildings;
            }

            // 确保每个城市都有yard（交付点）
            Building yardBuilding = buildingManager.GenerateBuilding("yard");
            if (yardBuilding != null)
            {
                buildings.Add(yardBuilding);
                Debug.Log($"Added yard building with size: {yardBuilding.Width}x{yardBuilding.Height}");
            }

            // 可能的建筑类型
            string[] buildingTypes = { "bar", "hotel", "exchange" };
            
            // 随机选择1-2个额外建筑
            int additionalCount = Random.Range(1, 3);
            List<string> availableTypes = new List<string>(buildingTypes);
            
            for (int i = 0; i < additionalCount && availableTypes.Count > 0; i++)
            {
                // 随机选择一个建筑类型
                int typeIndex = Random.Range(0, availableTypes.Count);
                string buildingType = availableTypes[typeIndex];
                availableTypes.RemoveAt(typeIndex); // 移除已选择的类型，确保不重复
                
                Building building = buildingManager.GenerateBuilding(buildingType);
                if (building != null)
                {
                    buildings.Add(building);
                    Debug.Log($"Added {buildingType} building with size: {building.Width}x{building.Height}");
                }
            }

            return buildings;
        }

        // 检查是否是城市区块
        public bool IsCityChunk(int worldX, int worldY)
        {
            return cities.Exists(city => city.Position.x == worldX && city.Position.y == worldY);
        }

        // 根据世界坐标获取城市
        public City GetCityAtPosition(int worldX, int worldY)
        {
            return cities.Find(city => city.Position.x == worldX && city.Position.y == worldY);
        }

        // 将城市布局注入到地形中 - 更新以与JS版本一致
        public void InjectCityLayout(TerrainType[,] terrain, int mapWidth, int currentX, int currentY)
        {
            if (cityLayouts.Count == 0) return;

            // 获取ASCIIConfig
            ASCIIConfig asciiConfig = GameController.Instance.ASCIIRenderer.asciiConfig;
            if (asciiConfig == null)
            {
                Debug.LogError("ASCIIConfig not found!");
                return;
            }

            // 随机选择一个布局
            string[,] selectedLayout = cityLayouts[Random.Range(0, cityLayouts.Count)];
            int layoutHeight = selectedLayout.GetLength(0);
            int layoutWidth = selectedLayout.GetLength(1);

            // 首先设置城市基础布局
            for (int y = 0; y < layoutHeight; y++)
            {
                for (int x = 0; x < layoutWidth; x++)
                {
                    int worldX = currentX + x;
                    int worldY = currentY + y;

                    if (worldX >= 0 && worldX < mapWidth && worldY >= 0 && worldY < terrain.GetLength(0))
                    {
                        char tileChar = selectedLayout[y, x][0];
                        var tileConfigs = asciiConfig.GetTileConfigsByChar(tileChar);
                        
                        if (tileConfigs != null && tileConfigs.Count > 0)
                        {
                            var config = tileConfigs[0];
                            terrain[worldY, worldX] = (TerrainType)config.id;
                        }
                    }
                }
            }

            // 获取当前城市
            City currentCity = GetCityAtPosition(currentX, currentY);
            if (currentCity == null)
            {
                Debug.LogError($"No city found at position ({currentX}, {currentY})");
                return;
            }

            // 确保城市有建筑
            if (currentCity.Buildings == null || currentCity.Buildings.Count == 0)
            {
                currentCity.Buildings = GenerateCityBuildings(20);
            }

            // 计算可用于放置建筑的区域（避开城墙和门）
            int buildingAreaStartX = 3;  // 增加边距，确保建筑不会太靠近城墙
            int buildingAreaStartY = 3;
            int buildingAreaWidth = layoutWidth - 6;  // 两边各留3格空间
            int buildingAreaHeight = layoutHeight - 6;

            // 跟踪已使用的区域
            bool[,] usedArea = new bool[layoutHeight, layoutWidth];

            // 放置建筑
            foreach (Building building in currentCity.Buildings)
            {
                bool placed = false;
                int maxAttempts = 30;  // 增加尝试次数
                int attempts = 0;

                while (!placed && attempts < maxAttempts)
                {
                    int buildingX = buildingAreaStartX + Random.Range(0, buildingAreaWidth - building.Width);
                    int buildingY = buildingAreaStartY + Random.Range(0, buildingAreaHeight - building.Height);

                    // 检查这个位置是否可以放置建筑
                    bool canPlace = true;
                    // 检查建筑周围的额外空间
                    for (int y = -1; y <= building.Height && canPlace; y++)
                    {
                        for (int x = -1; x <= building.Width && canPlace; x++)
                        {
                            int checkX = buildingX + x;
                            int checkY = buildingY + y;
                            
                            // 确保检查的位置在城市范围内
                            if (checkX >= 0 && checkX < layoutWidth && 
                                checkY >= 0 && checkY < layoutHeight)
                            {
                                if (usedArea[checkY, checkX])
                                {
                                    canPlace = false;
                                }
                            }
                        }
                    }

                    if (canPlace)
                    {
                        // 标记已使用的区域
                        for (int y = -1; y <= building.Height; y++)
                        {
                            for (int x = -1; x <= building.Width; x++)
                            {
                                int markX = buildingX + x;
                                int markY = buildingY + y;
                                if (markX >= 0 && markX < layoutWidth && 
                                    markY >= 0 && markY < layoutHeight)
                                {
                                    usedArea[markY, markX] = true;
                                }
                            }
                        }

                        // 放置建筑
                        building.LocalPosition = new Vector2Int(buildingX, buildingY);
                        for (int y = 0; y < building.Height; y++)
                        {
                            for (int x = 0; x < building.Width; x++)
                            {
                                int worldX = currentX + buildingX + x;
                                int worldY = currentY + buildingY + y;

                                if (worldX >= 0 && worldX < mapWidth && 
                                    worldY >= 0 && worldY < terrain.GetLength(0))
                                {
                                    char tileChar = building.Layout[y, x][0];
                                    var tileConfigs = asciiConfig.GetTileConfigsByChar(tileChar);
                                    
                                    if (tileConfigs != null && tileConfigs.Count > 0)
                                    {
                                        var config = tileConfigs[0];
                                        terrain[worldY, worldX] = (TerrainType)config.id;
                                    }
                                }
                            }
                        }
                        placed = true;
                        Debug.Log($"Successfully placed {building.Name} at ({buildingX}, {buildingY})");
                    }

                    attempts++;
                }

                if (!placed)
                {
                    Debug.LogWarning($"Failed to place building {building.Name} after {maxAttempts} attempts");
                }
            }
        }

        // 在城市周围添加树木 - 更新以与JS版本一致
        public void AddTreesAroundCity(TerrainType[,] terrain, int mapWidth, int currentX, int currentY)
        {
            City currentCity = GetCityAtPosition(currentX, currentY);
            if (currentCity == null) return;
            
            int citySize = 20; // 固定城市大小为20，与JS版本一致
            int startX = Mathf.FloorToInt((mapWidth - citySize) / 2f);
            int startY = Mathf.FloorToInt((terrain.GetLength(0) - citySize) / 2f);
            int endX = startX + citySize - 1;
            int endY = startY + citySize - 1;

            // 树木密度 - 值越低，树木越多 (与JS版本一致)
            float treeDensity = 0.8f;

            // 在城市墙外1-3格范围内添加树木
            for (int y = Mathf.Max(0, startY - 3); y <= Mathf.Min(terrain.GetLength(0) - 1, endY + 3); y++)
            {
                for (int x = Mathf.Max(0, startX - 3); x <= Mathf.Min(mapWidth - 1, endX + 3); x++)
                {
                    // 只在城市墙外添加树木，并且不改变原有地形
                    if (x < startX || x > endX || y < startY || y > endY)
                    {
                        // 随机决定是否放置树木，但不覆盖山脉和河流
                        if (Random.value > treeDensity && terrain[y, x] == TerrainType.Road)
                        {
                            terrain[y, x] = TerrainType.Tree;
                        }
                    }
                }
            }
        }

        // 处理城市内的行动点
        public bool HandleCityAction(Vector2Int playerPosition, TerrainType[,] terrain, int worldX, int worldY, 
                                     ref List<Task> tasks, ref int carriedCargo, ref float bitcoin, ref int strain)
        {
            City currentCity = GetCityAtPosition(worldX, worldY);
            if (currentCity == null) return false;

            // 查找玩家所在的建筑物
            Building building = FindBuildingAtPosition(currentCity, playerPosition.x, playerPosition.y, terrain);
            if (building == null) return false;

            // 获取城市在当前地图中的起始位置
            int mapHeight = terrain.GetLength(0);
            int mapWidth = terrain.GetLength(1);
            int startX = Mathf.FloorToInt((mapWidth - 20) / 2f);
            int startY = Mathf.FloorToInt((mapHeight - 20) / 2f);
            
            // 计算玩家在建筑内的局部坐标
            int localX = playerPosition.x - (startX + building.LocalPosition.x);
            int localY = playerPosition.y - (startY + building.LocalPosition.y);
            
            // 获取玩家位置在建筑内的特殊点类型
            string pointType = GetSpecialPointType(building, localX, localY);
            
            if (string.IsNullOrEmpty(pointType)) return false;

            // 根据特殊点类型执行不同行动
            switch (pointType)
            {
                case "task":
                    return HandleTaskPoint(ref tasks, ref carriedCargo, currentCity);
                case "delivery":
                    return HandleDeliveryPoint(ref tasks, ref carriedCargo, ref bitcoin);
                case "rest":
                    return HandleRestPoint(ref strain);
                default:
                    return false;
            }
        }

        // 查找玩家所在的建筑物
        private Building FindBuildingAtPosition(City city, int x, int y, TerrainType[,] terrain)
        {
            // 先获取城市在当前地图中的起始位置（基于20x20的固定尺寸）
            int mapHeight = terrain.GetLength(0);
            int mapWidth = terrain.GetLength(1);
            int startX = Mathf.FloorToInt((mapWidth - 20) / 2f);
            int startY = Mathf.FloorToInt((mapHeight - 20) / 2f);
            
            foreach (Building building in city.Buildings)
            {
                int buildingAbsX = startX + building.LocalPosition.x;
                int buildingAbsY = startY + building.LocalPosition.y;
                
                if (x >= buildingAbsX && x < buildingAbsX + building.Width &&
                    y >= buildingAbsY && y < buildingAbsY + building.Height)
                {
                    return building;
                }
            }
            return null;
        }

        // 获取特殊点类型
        private string GetSpecialPointType(Building building, int localX, int localY)
        {
            if (localX < 0 || localX >= building.Width || localY < 0 || localY >= building.Height)
                return null;

            // 使用GameController.Instance获取BuildingManager
            return GameController.Instance.BuildingManager.GetSpecialPointType(building, localX, localY);
        }

        // 处理任务接收点
        private bool HandleTaskPoint(ref List<Task> tasks, ref int carriedCargo, City fromCity)
        {
            DeathStrandingConfig gameConfig = GameController.Instance.GameConfig;
            
            if (carriedCargo >= gameConfig.maxCargo)
            {
                Debug.Log($"Cargo capacity full ({carriedCargo}/{gameConfig.maxCargo})!");
                return true;
            }

            // 检查是否已经接了这个城市的任务
            bool alreadyTaken = false;
            foreach (Task task in tasks)
            {
                if (task.Source == fromCity.Position)
                {
                    alreadyTaken = true;
                    break;
                }
            }

            if (!alreadyTaken)
            {
                List<City> otherCities = new List<City>();
                foreach (City city in cities)
                {
                    if (city.Position != fromCity.Position)
                    {
                        otherCities.Add(city);
                    }
                }

                if (otherCities.Count > 0)
                {
                    City targetCity = otherCities[Random.Range(0, otherCities.Count)];
                    Task newTask = new Task(
                        $"Deliver cargo from {fromCity.Name} to {targetCity.Name}",
                        fromCity.Position,
                        targetCity.Position,
                        1,
                        gameConfig.tasks.bitcoinReward
                    );
                    
                    tasks.Add(newTask);
                    carriedCargo++;

                    Debug.Log($"Task received! Target city: {citySymbols[$"{targetCity.Position.x},{targetCity.Position.y}"]}");
                    return true;
                }
            }
            else
            {
                Debug.Log("Task for this city already taken!");
                return true;
            }

            return false;
        }

        // 处理交付点
        private bool HandleDeliveryPoint(ref List<Task> tasks, ref int carriedCargo, ref float bitcoin)
        {
            bool delivered = false;
            DeathStrandingConfig gameConfig = GameController.Instance.GameConfig;
            
            Vector2Int currentPos = new Vector2Int(
                GameController.Instance.CurrentWorldX,
                GameController.Instance.CurrentWorldY);

            for (int i = tasks.Count - 1; i >= 0; i--)
            {
                Task task = tasks[i];
                if (task.Destination == currentPos)
                {
                    bitcoin += task.CargoAmount * gameConfig.tasks.bitcoinReward;
                    delivered = true;
                    tasks.RemoveAt(i);
                    carriedCargo = Mathf.Max(0, carriedCargo - 1);
                    break;
                }
            }

            Debug.Log(delivered ? "Task delivered successfully, Bitcoin earned!" : "No tasks to deliver here.");
            return true;
        }

        // 处理休息点
        private bool HandleRestPoint(ref int strain)
        {
            Debug.Log("You rested. Strain reduced to zero!");
            strain = 0;
            return true;
        }

        // 获取城市列表
        public List<City> GetCities()
        {
            return cities;
        }

        // 获取城市符号字典
        public Dictionary<string, string> GetCitySymbols()
        {
            return citySymbols;
        }

        public void GenerateCity(TerrainType[,] terrain, Vector2Int position, int index)
        {
            int citySize = Random.Range(MIN_CITY_SIZE, MAX_CITY_SIZE + 1);
            string citySymbol = index < citySymbolPool.Length ? citySymbolPool[index] : "C";

            City city = new City(citySymbol, position, citySize);

            // 生成城市边界
            for (int x = 0; x < citySize; x++)
            {
                for (int y = 0; y < citySize; y++)
                {
                    bool isEdge = x == 0 || x == citySize - 1 || y == 0 || y == citySize - 1;
                    bool isGate = false;

                    // 在四个方向的中点设置城门
                    if ((x == citySize / 2 && (y == 0 || y == citySize - 1)) ||
                        (y == citySize / 2 && (x == 0 || x == citySize - 1)))
                    {
                        isGate = true;
                    }

                    int worldX = position.x + x;
                    int worldY = position.y + y;

                    if (isEdge)
                    {
                        terrain[worldY, worldX] = isGate ? TerrainType.CityGate : TerrainType.CityWall;
                    }
                    else
                    {
                        terrain[worldY, worldX] = TerrainType.Road;
                    }
                }
            }

            // 生成建筑
            GenerateBuildings(terrain, city);
        }

        private void GenerateBuildings(TerrainType[,] terrain, City city)
        {
            // 在这里实现建筑生成逻辑
            // 1. Bar (小型或中型，必须有任务点，可能有休息点)
            // 2. Yard (中型、大型或L型，必须有交付点)
            // 3. Hotel (中型或大型，必须有休息点)
            // 4. Exchange (小型或中型，无特殊功能点)

            // 建筑生成逻辑...
        }

        public bool IsCityLocation(Vector2Int position, List<City> existingCities)
        {
            foreach (var city in existingCities)
            {
                float distance = Vector2.Distance(position, city.Position);
                if (distance < CITY_SPACING)
                {
                    return false;
                }
            }
            return true;
        }
    }
} 
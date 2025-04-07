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
        
        // 基础的空城市布局 - 将动态调整大小，复刻自JS版本
        private readonly string[,] baseCityLayout = new string[,] {
            {".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "#", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", "#", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "."},
            {".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", ".", "."}
        };

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

                    city.Buildings = GenerateCityBuildings();
                    
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
        private List<Building> GenerateCityBuildings()
        {
            List<Building> buildings = new List<Building>();
            string[] buildingTypes = { "bar", "yard", "hotel", "exchange" };

            // 通过GameController.Instance获取BuildingManager
            BuildingManager buildingManager = GameController.Instance.BuildingManager;
            
            // 确保每个城市都有yard
            Building yardBuilding = buildingManager.GenerateBuilding("yard");
            if (yardBuilding != null)
            {
                buildings.Add(yardBuilding);
            }

            // 从剩余建筑类型中随机选择，确保每种类型最多只有一个
            List<string> remainingTypes = new List<string>(buildingTypes);
            remainingTypes.Remove("yard");
            
            // 随机打乱顺序
            for (int i = 0; i < remainingTypes.Count; i++)
            {
                int randIndex = Random.Range(i, remainingTypes.Count);
                string temp = remainingTypes[i];
                remainingTypes[i] = remainingTypes[randIndex];
                remainingTypes[randIndex] = temp;
            }

            // 选择1-3个其他建筑
            int additionalCount = 1 + Random.Range(0, Mathf.Min(3, remainingTypes.Count));

            for (int i = 0; i < additionalCount; i++)
            {
                if (i < remainingTypes.Count)
                {
                    Building building = buildingManager.GenerateBuilding(remainingTypes[i]);
                    if (building != null)
                    {
                        buildings.Add(building);
                    }
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
        public void InjectCityLayout(TerrainType[,] terrain, int mapSize, int currentX, int currentY)
        {
            City currentCity = GetCityAtPosition(currentX, currentY);
            if (currentCity == null) return;

            // 为城市清理空间
            int citySize = 20; // 固定城市大小为20，与JS版本一致
            int startX = Mathf.FloorToInt((mapSize - citySize) / 2f);
            int startY = Mathf.FloorToInt((mapSize - citySize) / 2f);

            // 添加城市外墙
            for (int x = startX; x < startX + citySize; x++)
            {
                terrain[startY, x] = TerrainType.City;  // 上墙
                terrain[startY + citySize - 1, x] = TerrainType.City;  // 下墙
            }

            for (int y = startY; y < startY + citySize; y++)
            {
                terrain[y, startX] = TerrainType.City;  // 左墙
                terrain[y, startX + citySize - 1] = TerrainType.City;  // 右墙
            }

            // 在墙上创建四个入口(四个方向的中间位置)
            int midX = startX + Mathf.FloorToInt(citySize / 2f);
            int midY = startY + Mathf.FloorToInt(citySize / 2f);

            // 设置入口处的地形为Road (与JavaScript版本一致)
            terrain[startY, midX] = TerrainType.Road;  // 上方门
            terrain[midY, startX + citySize - 1] = TerrainType.Road;  // 右方门
            terrain[startY + citySize - 1, midX] = TerrainType.Road;  // 下方门
            terrain[midY, startX] = TerrainType.Road;  // 左方门

            // 放置城市内部建筑
            PlaceBuildingsInCity(currentCity, terrain, startX, startY, citySize);
        }

        // 在城市内放置建筑 - 更新以与JS版本一致
        private void PlaceBuildingsInCity(City city, TerrainType[,] terrain, int startX, int startY, int citySize)
        {
            int buildingX = startX + 2;
            int buildingY = startY + 2;

            foreach (Building building in city.Buildings)
            {
                // 存储建筑物位置坐标
                Vector2Int localPos = new Vector2Int(buildingX - startX, buildingY - startY);
                building.LocalPosition = localPos;

                // 放置建筑名称在建筑物上方
                if (building.Name.Length > 0)
                {
                    int nameX = buildingX + Mathf.FloorToInt((building.Width - building.Name.Length) / 2f);
                    for (int i = 0; i < building.Name.Length && i + nameX < terrain.GetLength(1); i++)
                    {
                        terrain[buildingY - 1, nameX + i] = TerrainType.City;
                    }
                }

                // 放置建筑布局
                for (int y = 0; y < building.Height; y++)
                {
                    for (int x = 0; x < building.Width; x++)
                    {
                        if (buildingX + x < terrain.GetLength(1) && buildingY + y < terrain.GetLength(0))
                        {
                            // 设置建筑物墙壁和地板
                            if (building.Layout[y, x] == "#")
                                terrain[buildingY + y, buildingX + x] = TerrainType.City;
                            else if (building.Layout[y, x] == ".")
                                terrain[buildingY + y, buildingX + x] = TerrainType.Road;
                            else if (building.Layout[y, x] == "|" || building.Layout[y, x] == "-")
                                terrain[buildingY + y, buildingX + x] = TerrainType.Road; // 门
                            else if (building.Layout[y, x] == "■" || building.Layout[y, x] == "□" || building.Layout[y, x] == "+")
                                terrain[buildingY + y, buildingX + x] = TerrainType.Road; // 特殊点
                        }
                    }
                }

                // 更新下一个建筑的位置
                buildingX += building.Width + 2;
                if (buildingX + building.Width >= startX + citySize - 1)
                {
                    buildingX = startX + 2;
                    buildingY += building.Height + 2;
                }
            }
        }

        // 在城市周围添加树木 - 更新以与JS版本一致
        public void AddTreesAroundCity(TerrainType[,] terrain, int mapSize, int currentX, int currentY)
        {
            City currentCity = GetCityAtPosition(currentX, currentY);
            if (currentCity == null) return;
            
            int citySize = 20; // 固定城市大小为20，与JS版本一致
            int startX = Mathf.FloorToInt((mapSize - citySize) / 2f);
            int startY = Mathf.FloorToInt((mapSize - citySize) / 2f);
            int endX = startX + citySize - 1;
            int endY = startY + citySize - 1;

            // 树木密度 - 值越低，树木越多 (与JS版本一致)
            float treeDensity = 0.8f;

            // 在城市墙外1-3格范围内添加树木
            for (int y = Mathf.Max(0, startY - 3); y <= Mathf.Min(mapSize - 1, endY + 3); y++)
            {
                for (int x = Mathf.Max(0, startX - 3); x <= Mathf.Min(mapSize - 1, endX + 3); x++)
                {
                    // 只在城市墙外添加树木
                    if (x < startX || x > endX || y < startY || y > endY)
                    {
                        // 随机决定是否放置树木
                        if (Random.value > treeDensity && terrain[y, x] == TerrainType.Grass)
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
            int mapSize = terrain.GetLength(0);
            int startX = Mathf.FloorToInt((mapSize - 20) / 2f);
            int startY = Mathf.FloorToInt((mapSize - 20) / 2f);
            
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
            int mapSize = terrain.GetLength(0);
            int startX = Mathf.FloorToInt((mapSize - 20) / 2f);
            int startY = Mathf.FloorToInt((mapSize - 20) / 2f);
            
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
    }
} 
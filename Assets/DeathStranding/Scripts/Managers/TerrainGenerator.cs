using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections.Generic;

namespace ALUNGAMES
{
    public class TerrainGenerator : MonoBehaviour
    {
        private TerrainType[,] terrain;
        private int mapWidth;
        private int mapHeight;
        
        // 新增地形管线组件
        private HeightMapGenerator heightGenerator;
        private HydraulicErosion erosionProcessor;
        private BiomeDistributor biomeDistributor;

        private void Awake()
        {
            heightGenerator = new HeightMapGenerator();
            erosionProcessor = new HydraulicErosion();
            biomeDistributor = new BiomeDistributor();
        }

        // 生成地形
        public TerrainType[,] GenerateTerrain(int mapWidth, int mapHeight, int currentWorldX, int currentWorldY)
        {
            try
            {
                // 检查参数有效性
                if (mapWidth <= 0 || mapHeight <= 0)
                {
                    Debug.LogError($"TerrainGenerator.GenerateTerrain: 无效的地图大小: {mapWidth}x{mapHeight}，使用默认值40x40");
                    mapWidth = 40;
                    mapHeight = 40;
                }
                
                this.mapWidth = mapWidth;
                this.mapHeight = mapHeight;
                terrain = new TerrainType[mapHeight, mapWidth];
                
                // 通过GameController.Instance获取依赖组件
                DeathStrandingConfig gameConfig = GameController.Instance.GameConfig;
                CityManager cityManager = GameController.Instance.CityManager;
                
                // 检查必要组件
                if (gameConfig == null)
                {
                    Debug.LogWarning("TerrainGenerator: gameConfig为null，使用默认配置");
                    gameConfig = ScriptableObject.CreateInstance<DeathStrandingConfig>();
                }
                
                // 如果是默认种子值(100f)，则生成随机种子
                if (Mathf.Approximately(gameConfig.worldSeed, 100f))
                {
                    gameConfig.worldSeed = Random.Range(0f, 10000f);
                    Debug.Log($"生成随机世界种子: {gameConfig.worldSeed}");
                }
                
                if (cityManager == null)
                {
                    Debug.LogWarning("TerrainGenerator: cityManager为null，将不生成城市");
                }
                
                // 初始化地形管线
                heightGenerator.Initialize(mapWidth, mapHeight, gameConfig, currentWorldX, currentWorldY);
                float[,] heightMap = heightGenerator.Generate();
                erosionProcessor.Erode(heightMap, gameConfig);

                // 归一化高度图到0-1范围
                float minHeight = float.MaxValue;
                float maxHeight = float.MinValue;
                for (int y = 0; y < mapHeight; y++)
                {
                    for (int x = 0; x < mapWidth; x++)
                    {
                        minHeight = Mathf.Min(minHeight, heightMap[y, x]);
                        maxHeight = Mathf.Max(maxHeight, heightMap[y, x]);
                    }
                }
                float range = maxHeight - minHeight;
                for (int y = 0; y < mapHeight; y++)
                {
                    for (int x = 0; x < mapWidth; x++)
                    {
                        heightMap[y, x] = (heightMap[y, x] - minHeight) / range;
                    }
                }
                Debug.Log($"归一化后高度图范围: {minHeight}-{maxHeight}");
                
                // 初始化为空地
                for (int y = 0; y < mapHeight; y++)
                {
                    for (int x = 0; x < mapWidth; x++)
                    {
                        terrain[y, x] = TerrainType.Empty;
                    }
                }
                
                // 检查当前区块是否为城市
                bool isCity = false;
                if (cityManager != null)
                {
                    isCity = cityManager.IsCityChunk(currentWorldX, currentWorldY);
                }
                
                // 生成山脉和草地，但避开城市区域
                if (!isCity)
                {
                    GenerateMountainsAndGrass(heightMap);
                    
                    // 生成河流
                    int riverCount = gameConfig.riverCount > 0 ? gameConfig.riverCount : 2;
                    for (int i = 0; i < riverCount; i++)
                    {
                        CarveRiver(Random.Range(0, mapWidth), Random.Range(0, mapHeight), heightMap);
                    }
                    
                    // 生成树木
                    GenerateTrees();
                }
                else if (cityManager != null)
                {
                    // 如果是城市区块，注入城市布局
                    cityManager.InjectCityLayout(terrain, mapWidth, currentWorldX, currentWorldY);
                    
                    // 在城市周围添加树木，使用较小的尺寸作为mapSize以确保安全
                    int minSize = Mathf.Min(mapWidth, mapHeight);
                    cityManager.AddTreesAroundCity(terrain, minSize, currentWorldX, currentWorldY);
                }
                
                return terrain;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"GenerateTerrain发生错误: {e.Message}\n{e.StackTrace}");
                
                // 创建应急地形
                TerrainType[,] fallbackTerrain = new TerrainType[mapWidth > 0 ? mapWidth : 40, mapHeight > 0 ? mapHeight : 40];
                for (int y = 0; y < fallbackTerrain.GetLength(0); y++)
                {
                    for (int x = 0; x < fallbackTerrain.GetLength(1); x++)
                    {
                        fallbackTerrain[y, x] = TerrainType.Road;
                    }
                }
                
                return fallbackTerrain;
            }
        }
        
        // 新增地形管线类
        private class HeightMapGenerator 
        {
            private int width;
            private int height;
            private DeathStrandingConfig config;
            
            private int worldX;
            private int worldY;
            
            public void Initialize(int width, int height, DeathStrandingConfig config, int worldX = 0, int worldY = 0) 
            {
                this.width = width;
                this.height = height;
                this.config = config;
                this.worldX = worldX;
                this.worldY = worldY;
            }
            
            public float[,] Generate() 
            {
                float[,] heightMap = new float[height, width];
                float seed = config.worldSeed;
                
                // 使用世界坐标生成连续噪声
                float worldScale = 0.1f;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float worldPixelX = (worldX * width + x) * worldScale;
                        float worldPixelY = (worldY * height + y) * worldScale;
                        heightMap[y, x] = Mathf.PerlinNoise(worldPixelX + seed, worldPixelY + seed);
                    }
                }
                return heightMap;
            }
        }
        
        private class HydraulicErosion
        {
            public void Erode(float[,] heightMap, DeathStrandingConfig config) 
            {
                // 暂为占位实现
                Debug.Log("Hydraulic erosion processing (placeholder)");
            }
        }
        
        private class BiomeDistributor
        {
            public TerrainType[,] Distribute(float[,] heightMap, DeathStrandingConfig config) 
            {
                int height = heightMap.GetLength(0);
                int width = heightMap.GetLength(1);
                TerrainType[,] biomeMap = new TerrainType[height, width];
                
                // 临时实现 - 后续将根据海拔和温湿度重写
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        biomeMap[y, x] = TerrainType.Road;
                    }
                }
                return biomeMap;
            }
        }
        
        // 生成山脉和草地
        private void GenerateMountainsAndGrass(float[,] heightMap)
        {
            DeathStrandingConfig gameConfig = GameController.Instance.GameConfig;
            
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    if (heightMap[y, x] > gameConfig.mountainThreshold)
                    {
                        // 添加随机性，不是所有超过阈值的点都生成山地
                        if (Random.value < 0.7f)  // 70%的概率生成山地
                        {
                            // 检查周围是否已经有山地，避免连续生成
                            bool hasNearbyMountain = false;
                            for (int dy = -1; dy <= 1 && !hasNearbyMountain; dy++)
                            {
                                for (int dx = -1; dx <= 1; dx++)
                                {
                                    int nx = x + dx;
                                    int ny = y + dy;
                                    if (nx >= 0 && nx < mapWidth && ny >= 0 && ny < mapHeight)
                                    {
                                        if (terrain[ny, nx] == TerrainType.Mountain)
                                        {
                                            hasNearbyMountain = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            
                            if (!hasNearbyMountain)
                            {
                                terrain[y, x] = TerrainType.Mountain;
                            }
                            else
                            {
                                terrain[y, x] = TerrainType.Road;
                            }
                        }
                        else
                        {
                            terrain[y, x] = TerrainType.Road;
                        }
                    }
                    else if (heightMap[y, x] > gameConfig.grassThreshold)
                    {
                        // 30%的概率生成草地
                        terrain[y, x] = Random.value < gameConfig.grassChance ? TerrainType.Grass : TerrainType.Road;
                    }
                }
            }
        }
        
        // 雕刻河流
        private void CarveRiver(int startX, int startY, float[,] heightMap)
        {
            // 参数边界检查
            if (heightMap == null)
            {
                Debug.LogWarning("CarveRiver: heightMap为null");
                return;
            }
            
            // 设置第一个河流点
            if (startX < 0 || startX >= mapWidth || startY < 0 || startY >= mapHeight)
            {
                Debug.LogWarning($"CarveRiver: 起始点({startX},{startY})超出地图范围({mapWidth},{mapHeight})");
                return;
            }
                
            terrain[startY, startX] = TerrainType.Water;
            
            try
            {
                // 随机选择一个方向: 0=右, 1=上, 2=左, 3=下
                int direction = Random.Range(0, 4);
                int[] dx = { 1, 0, -1, 0 };
                int[] dy = { 0, -1, 0, 1 };
                
                // 随机确定长度
                DeathStrandingConfig gameConfig = GameController.Instance.GameConfig;
                int minLength = gameConfig != null && gameConfig.riverMinLength > 0 ? gameConfig.riverMinLength : 5;
                int maxLength = Mathf.Max(mapWidth - startX, mapHeight - startY, startX, startY);
                maxLength = Mathf.Max(minLength + 1, maxLength); // 确保maxLength大于minLength
                int length = minLength + Random.Range(0, maxLength - minLength + 1);
                
                // 沿着选定方向延伸河流
                for (int i = 1; i <= length; i++)
                {
                    int nx = startX + dx[direction] * i;
                    int ny = startY + dy[direction] * i;
                    
                    // 检查边界
                    if (nx < 0 || nx >= mapWidth || ny < 0 || ny >= mapHeight)
                        break;
                    
                    terrain[ny, nx] = TerrainType.Water;
                    
                    // 随机分支河流
                    float branchChance = gameConfig != null ? gameConfig.riverBranchChance : 0.2f;
                    if (Random.value < branchChance)
                    {
                        BranchRiver(nx, ny, dx[direction], dy[direction]);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CarveRiver发生错误: {e.Message}");
            }
        }
        
        // 生成河流分支
        private void BranchRiver(int x, int y, int parentDx, int parentDy)
        {
            // 选择与父河流不同的方向
            int[] dx = { 1, 0, -1, 0 };
            int[] dy = { 0, -1, 0, 1 };
            
            System.Collections.Generic.List<System.Tuple<int, int>> possibleDirs = 
                new System.Collections.Generic.List<System.Tuple<int, int>>();
                
            for (int dir = 0; dir < 4; dir++)
            {
                int dirX = dx[dir];
                int dirY = dy[dir];
                
                // 确保不是父河流的反方向或相同方向
                if (!(dirX == parentDx && dirY == parentDy) && !(dirX == -parentDx && dirY == -parentDy))
                {
                    possibleDirs.Add(new System.Tuple<int, int>(dirX, dirY));
                }
            }
            
            if (possibleDirs.Count == 0) return;
            
            // 随机选择一个可能的方向
            var randomDir = possibleDirs[Random.Range(0, possibleDirs.Count)];
            int newDx = randomDir.Item1;
            int newDy = randomDir.Item2;
            
            // 随机长度，3-7格
            int length = 3 + Random.Range(0, 5);
            
            for (int i = 1; i <= length; i++)
            {
                int nx = x + newDx * i;
                int ny = y + newDy * i;
                
                // 检查边界
                if (nx < 0 || nx >= mapWidth || ny < 0 || ny >= mapHeight)
                    break;
                
                terrain[ny, nx] = TerrainType.Water;
            }
        }
        
        // 生成树木
        private void GenerateTrees()
        {
            try
            {
                // 参考game.js中的generateTrees方法实现
                // 确定这张地图的树木总权重
                DeathStrandingConfig gameConfig = GameController.Instance.GameConfig;
                int treeMaxWeight = gameConfig != null ? gameConfig.treeMaxCount : 20;
                int smallTreeWeight = gameConfig != null ? gameConfig.smallTreeWeight : 1;
                int largeTreeWeight = gameConfig != null ? gameConfig.largeTreeWeight : 4;
                float largeTreeChance = gameConfig != null ? gameConfig.largeTreeChance : 0.3f;
                
                int totalTreeWeight = Random.Range(0, treeMaxWeight + 1);
                int currentWeight = 0;
                
                // 尝试放置树木
                int attempts = 0;
                int maxAttempts = 200;
                
                Debug.Log($"生成树木: 目标权重={totalTreeWeight}, 大树概率={largeTreeChance}");
                
                while (currentWeight < totalTreeWeight && attempts < maxAttempts)
                {
                    attempts++;
                    
                    // 随机位置
                    int x = Random.Range(0, mapWidth);
                    int y = Random.Range(0, mapHeight);
                    
                    // 确保不会越界
                    if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                        continue;
                        
                    // 检查是否是可放置树木的地形
                    if (terrain[y, x] != TerrainType.Grass && terrain[y, x] != TerrainType.Road)
                        continue;
                    
                    // 决定生成小树(权重1)还是大树(权重4)
                    bool isLargeTree = Random.value < largeTreeChance && 
                                      currentWeight <= totalTreeWeight - largeTreeWeight;
                    
                    if (isLargeTree)
                    {
                        // 检查是否有足够空间放置大树(2x2)
                        if (x + 1 >= mapWidth || y + 1 >= mapHeight)
                            continue;
                        
                        // 检查2x2区域是否有其他障碍物
                        bool canPlaceLargeTree = true;
                        for (int dy = 0; dy < 2; dy++)
                        {
                            for (int dx = 0; dx < 2; dx++)
                            {
                                int tx = x + dx;
                                int ty = y + dy;
                                
                                if (tx < 0 || tx >= mapWidth || ty < 0 || ty >= mapHeight ||
                                    (terrain[ty, tx] != TerrainType.Grass && terrain[ty, tx] != TerrainType.Road))
                                {
                                    canPlaceLargeTree = false;
                                    break;
                                }
                            }
                            if (!canPlaceLargeTree) break;
                        }
                        
                        if (canPlaceLargeTree)
                        {
                            // 放置2x2的大树
                            for (int dy = 0; dy < 2; dy++)
                            {
                                for (int dx = 0; dx < 2; dx++)
                                {
                                    terrain[y + dy, x + dx] = TerrainType.LargeTree;
                                }
                            }
                            currentWeight += largeTreeWeight;
                            
                            if (Random.value < 0.1f) // 偶尔记录日志便于调试
                                Debug.Log($"放置大树 在 ({x},{y}), 当前权重: {currentWeight}/{totalTreeWeight}");
                        }
                    }
                    else
                    {
                        // 放置小树
                        terrain[y, x] = TerrainType.Tree;
                        currentWeight += smallTreeWeight;
                    }
                }
                
                Debug.Log($"树木生成完成: 最终权重={currentWeight}/{totalTreeWeight}, 尝试次数={attempts}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"GenerateTrees发生错误: {e.Message}\n{e.StackTrace}");
            }
        }
        
        // 平滑噪声函数
        private float SmoothNoise(float x, float y, float seed)
        {
            float corners = (Noise(x - 1, y - 1, seed) + Noise(x + 1, y - 1, seed) +
                           Noise(x - 1, y + 1, seed) + Noise(x + 1, y + 1, seed)) / 16f;
            float sides = (Noise(x - 1, y, seed) + Noise(x + 1, y, seed) +
                         Noise(x, y - 1, seed) + Noise(x, y + 1, seed)) / 8f;
            float center = Noise(x, y, seed) / 4f;
            
            return corners + sides + center;
        }
        
        // 基础噪声函数
        private float Noise(float x, float y, float seed)
        {
            float n = x + y * 57 + seed * 131;
            return Mathf.Sin(n * 12.9898f) * 43758.5453f % 1;
        }
    }
}

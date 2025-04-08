using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace ALUNGAMES
{
    public class ASCIIRenderer : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        public ASCIIConfig asciiConfig;
        [SerializeField] private bool showTileIds = false; // 调试开关：显示tile的ID

        private VisualElement root;
        private VisualElement gameMap;
        private VisualElement worldMap;

        // 地图元素引用
        private Label[,] mapElements;

        private void Awake()
        {
            // 加载配置
            if (asciiConfig == null)
            {
                asciiConfig = Resources.Load<ASCIIConfig>("ASCIIConfig");
            }

            // 初始化地形配置字典
            // terrainConfigs = new Dictionary<TerrainType, ASCIIConfig.TileConfig>();
            // foreach (var config in asciiConfig.tiles)
            // {
            //     if (config.id >= 0 && config.id < System.Enum.GetValues(typeof(TerrainType)).Length)
            //     {
            //         terrainConfigs[(TerrainType)config.id] = config;
            //     }
            // }
        }

        private void OnEnable()
        {
            Initialize();

            // 订阅区块变化事件 - 这时才重建地图
            GameController.Instance.PlayerController.OnPlayerChangedChunk += UpdateWorldAndMaps;
        }

        private void OnDisable()
        {
            // 取消订阅事件
            if (GameController.Instance != null && GameController.Instance.PlayerController != null)
            {
                GameController.Instance.PlayerController.OnPlayerChangedChunk -= UpdateWorldAndMaps;
            }
        }

        // 初始化
        public void Initialize()
        {
            if (uiDocument == null) return;

            root = uiDocument.rootVisualElement;

            // 获取地图元素
            gameMap = root.Q<VisualElement>("game-map");
            worldMap = root.Q<VisualElement>("world-map");

            // 初始渲染
            RenderMap();
            RenderWorldMap();
        }

        // 重量级更新 - 在区块变化时重建地图
        public void UpdateWorldAndMaps(int newX, int newY)
        {
            // 完整重建地图和世界地图
            RenderMap();
            RenderWorldMap();
        }

        // 只更新玩家位置标记，不重新生成整个地图
        public void UpdatePlayerPositionOnMap()
        {
            var playerController = GameController.Instance.PlayerController;
            if (playerController == null || mapElements == null)
                return;

            try
            {
                TerrainType[,] terrain = GameController.Instance.GetCurrentTerrain();
                if (terrain == null)
                    return;

                int terrainSizeY = terrain.GetLength(0);
                int terrainSizeX = terrain.GetLength(1);

                bool isInCity = GameController.Instance.CityManager.IsCityChunk(
                    GameController.Instance.CurrentWorldX,
                    GameController.Instance.CurrentWorldY);

                // 先重置所有单元格为原始地形显示
                for (int y = 0; y < terrainSizeY; y++)
                {
                    for (int x = 0; x < terrainSizeX; x++)
                    {
                        if (mapElements[y, x] != null)
                        {
                            RenderCell(mapElements[y, x], terrain[y, x], isInCity);
                        }
                    }
                }

                // 标记玩家位置
                int playerX = playerController.PlayerPosition.x;
                int playerY = playerController.PlayerPosition.y;

                // 边界检查
                if (playerX >= 0 && playerX < terrainSizeX && playerY >= 0 && playerY < terrainSizeY)
                {
                    if (mapElements[playerY, playerX] != null)
                    {
                        RenderCell(mapElements[playerY, playerX], terrain[playerY, playerX], isInCity, true);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"更新玩家位置标记时出错: {e.Message}");
            }
        }

        // 渲染游戏地图
        public void RenderMap()
        {
            try
            {
                // 检查gameMap是否存在
                if (gameMap == null)
                {
                    Debug.LogError("RenderMap: gameMap为null");
                    return;
                }

                // 清空地图
                gameMap.Clear();

                // 获取当前地形
                TerrainType[,] terrain = GameController.Instance.GetCurrentTerrain();
                if (terrain == null)
                {
                    Debug.LogError("RenderMap: 地形为null");
                    return;
                }

                // 获取实际地形大小
                int terrainSizeY = terrain.GetLength(0);
                int terrainSizeX = terrain.GetLength(1);

                // 确保地形尺寸有效
                if (terrainSizeY <= 0 || terrainSizeX <= 0)
                {
                    Debug.LogError($"RenderMap: 无效的地形尺寸: {terrainSizeY}x{terrainSizeX}");
                    return;
                }

                // 调整mapElements数组大小
                mapElements = new Label[terrainSizeY, terrainSizeX];

                // 检查是否在城市中
                bool isInCity = GameController.Instance.CityManager.IsCityChunk(
                    GameController.Instance.CurrentWorldX,
                    GameController.Instance.CurrentWorldY);

                // 创建行容器
                for (int y = 0; y < terrainSizeY; y++)
                {
                    var row = new VisualElement();
                    row.style.flexDirection = FlexDirection.Row;
                    row.style.height = 15; // 行高固定

                    for (int x = 0; x < terrainSizeX; x++)
                    {
                        var cell = new Label();
                        cell.style.width = 15;
                        cell.style.height = 15;
                        cell.style.unityTextAlign = TextAnchor.MiddleCenter;
                        cell.style.fontSize = 14;

                        // 使用新的渲染方法
                        RenderCell(cell, terrain[y, x], isInCity);

                        // 存储引用
                        mapElements[y, x] = cell;
                        row.Add(cell);
                    }

                    gameMap.Add(row);
                }

                // 如果有玩家，标记玩家位置
                var playerController = GameController.Instance.PlayerController;
                if (playerController != null)
                {
                    int playerX = playerController.PlayerPosition.x;
                    int playerY = playerController.PlayerPosition.y;

                    // 边界检查
                    if (playerX >= 0 && playerX < terrainSizeX && playerY >= 0 && playerY < terrainSizeY)
                    {
                        if (mapElements[playerY, playerX] != null)
                        {
                            RenderCell(mapElements[playerY, playerX], terrain[playerY, playerX], isInCity, true);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"渲染地图时发生错误: {e.Message}\n{e.StackTrace}");
            }
        }

        // 渲染世界地图
        public void RenderWorldMap()
        {
            try
            {
                // 检查worldMap是否存在
                if (worldMap == null)
                {
                    Debug.LogError("RenderWorldMap: worldMap为null");
                    return;
                }

                // 清空世界地图
                worldMap.Clear();

                // 通过GameController获取依赖
                var playerController = GameController.Instance.PlayerController;
                var cityManager = GameController.Instance.CityManager;
                var gameConfig = GameController.Instance.GameConfig;

                // 检查依赖组件
                if (playerController == null || cityManager == null)
                {
                    Debug.LogError("RenderWorldMap: playerController或cityManager为null");
                    return;
                }

                // 从gameConfig获取世界尺寸，如果不可用则使用默认值
                int worldWidth = gameConfig != null ? gameConfig.worldWidth : 10;
                int worldHeight = gameConfig != null ? gameConfig.worldHeight : 10;

                // 获取玩家当前的世界位置
                int playerWorldX = playerController.GetCurrentWorldX();
                int playerWorldY = playerController.GetCurrentWorldY();

                try
                {
                    // 创建世界地图行
                    for (int y = 0; y < worldHeight; y++)
                    {
                        var row = new VisualElement();
                        row.style.flexDirection = FlexDirection.Row;
                        row.style.height = 20; // 行高

                        for (int x = 0; x < worldWidth; x++)
                        {
                            var cell = new Label();
                            cell.style.width = 20;
                            cell.style.height = 20;
                            cell.style.unityTextAlign = TextAnchor.MiddleCenter;
                            cell.style.fontSize = 14;

                            // 默认显示为空格
                            cell.text = " ";
                            cell.style.color = new Color(0.5f, 0.5f, 0.5f);

                            // 标记城市
                            bool isCity = false;
                            if (cityManager != null)
                            {
                                isCity = cityManager.IsCityChunk(x, y);
                                if (isCity)
                                {
                                    City city = cityManager.GetCityAtPosition(x, y);
                                    if (city != null)
                                    {
                                        // 使用city.Symbol属性
                                        if (city.Symbol != '?')
                                        {
                                            cell.text = city.Symbol.ToString();
                                        }
                                        else
                                        {
                                            // 如果符号是默认值，使用"C"表示城市中心
                                            cell.text = "C";
                                        }
                                        cell.style.color = Color.white;
                                    }
                                    else
                                    {
                                        cell.text = "C";
                                        cell.style.color = Color.white;
                                    }
                                }
                            }

                            // 标记玩家位置
                            if (x == playerWorldX && y == playerWorldY)
                            {
                                cell.text = "P";
                                cell.style.color = Color.yellow;
                                cell.style.unityFontStyleAndWeight = FontStyle.Bold;
                            }

                            row.Add(cell);
                        }

                        worldMap.Add(row);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"渲染世界地图单元格时出错: {e.Message}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"渲染世界地图时发生错误: {e.Message}\n{e.StackTrace}");
            }
        }

        // 获取地形对应字符
        private string GetTerrainChar(TerrainType terrain)
        {
            if (showTileIds)
                return GetTileConfig((int)terrain).id.ToString();
            else
                return GetTileConfig((int)terrain).asciiChar.ToString();
        }

        // 获取地形颜色
        private Color GetTerrainColor(TerrainType terrain, bool isInCity)
        {
            return GetTileConfig((int)terrain).color;
        }
        ASCIIConfig.TileConfig GetTileConfig(int id)
        {
            return asciiConfig.GetTileConfig(id);
        }
        // 更新渲染单元格的方法
        private void RenderCell(Label cell, TerrainType terrain, bool isInCity, bool isPlayer = false)
        {
            if (cell == null) return;

            if (isPlayer)
            {
                cell.text = GetTileConfig(-1).asciiChar.ToString();
                // 玩家使用黄色
                cell.style.color = GetTileConfig(-1).color;
            }
            else
            {
                cell.text = GetTerrainChar(terrain);
                cell.style.color = GetTerrainColor(terrain, isInCity);
            }
        }
    }
}
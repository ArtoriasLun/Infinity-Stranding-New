using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ALUNGAMES.Editor
{
    public class ASCIIConfigGenerator : EditorWindow
    {
        private BuildingManager buildingManager;
        private CityManager cityManager;
        private TerrainGenerator terrainGenerator;
        private TaskManager taskManager;
        private GameController gameController;
        
        private string configName = "ASCIIConfig";
        private string outputPath = "Assets/DeathStranding/Resources";
        private bool useDefaultConfig = true;
        
        // 存储提取的配置数据
        private List<ASCIIConfig.TileConfig> tileConfigs = new List<ASCIIConfig.TileConfig>();
        
        [MenuItem("😊/ASCII配置生成器")]
        public static void ShowWindow()
        {
            GetWindow<ASCIIConfigGenerator>("ASCII配置生成器");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("ASCII配置自动生成工具", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            EditorGUILayout.HelpBox("此工具将生成ASCII配置资源", MessageType.Info);
            EditorGUILayout.Space(10);
            
            // 配置选项
            configName = EditorGUILayout.TextField("配置名称", configName);
            outputPath = EditorGUILayout.TextField("输出路径", outputPath);
            useDefaultConfig = EditorGUILayout.Toggle("使用默认配置", useDefaultConfig);
            
            EditorGUILayout.Space(10);
            
            if (!useDefaultConfig)
            {
            // 引用对象字段
            buildingManager = (BuildingManager)EditorGUILayout.ObjectField("建筑管理器", buildingManager, typeof(BuildingManager), true);
            cityManager = (CityManager)EditorGUILayout.ObjectField("城市管理器", cityManager, typeof(CityManager), true);
            terrainGenerator = (TerrainGenerator)EditorGUILayout.ObjectField("地形生成器", terrainGenerator, typeof(TerrainGenerator), true);
            taskManager = (TaskManager)EditorGUILayout.ObjectField("任务管理器", taskManager, typeof(TaskManager), true);
            gameController = (GameController)EditorGUILayout.ObjectField("游戏控制器", gameController, typeof(GameController), true);
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("在场景中查找组件"))
            {
                FindComponentsInScene();
                }
            }
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("生成ASCII配置"))
            {
                if (useDefaultConfig)
                {
                    GenerateDefaultConfig();
            }
                else
            {
                    GenerateConfigFromScene();
                }
            }
        }
        
        private void FindComponentsInScene()
        {
            buildingManager = FindObjectOfType<BuildingManager>();
            cityManager = FindObjectOfType<CityManager>();
            terrainGenerator = FindObjectOfType<TerrainGenerator>();
            taskManager = FindObjectOfType<TaskManager>();
            gameController = FindObjectOfType<GameController>();
            
            if (buildingManager == null && cityManager == null && terrainGenerator == null)
            {
                EditorUtility.DisplayDialog("未找到组件", "在场景中未找到所需的管理器组件。请先设置游戏场景。", "确定");
            }
        }
        
        private void GenerateDefaultConfig()
        {
            var config = ScriptableObject.CreateInstance<ASCIIConfig>();
            List<ASCIIConfig.TileConfig> configs = new List<ASCIIConfig.TileConfig>();
            
            // 基础地形配置
            configs.Add(CreateTileConfig(0, "Empty", ' ', Color.white, true, 1));
            configs.Add(CreateTileConfig(1, "Road", '.', Color.white, true, 1));
            configs.Add(CreateTileConfig(2, "Grass", '*', Color.white, true, 1));
            configs.Add(CreateTileConfig(3, "Mountain", '^', Color.white, true, 3, 0.3f, 10));
            configs.Add(CreateTileConfig(4, "Water", '~', Color.white, true, 2, 0.5f, 10));
            
            // 城市相关配置
            configs.Add(CreateTileConfig(5, "City Wall", '#', Color.white, false, 0, isWall: true));
            configs.Add(CreateTileConfig(6, "City Gate", '|', Color.white, true, 1, isGate: true));
            configs.Add(CreateTileConfig(7, "Building Wall", '#', Color.gray, false, 0, isWall: true));
            configs.Add(CreateTileConfig(8, "Building Gate", '|', Color.gray, true, 1, isGate: true));
            
            // 特殊点配置
            configs.Add(CreateTileConfig(9, "Task Point", '■', Color.yellow, true, 1, isSpecialPoint: true));
            configs.Add(CreateTileConfig(10, "Delivery Point", '□', Color.green, true, 1, isSpecialPoint: true));
            configs.Add(CreateTileConfig(11, "Rest Point", '+', Color.cyan, true, 1, isSpecialPoint: true));
            
            // 自然物体配置
            configs.Add(CreateTileConfig(12, "Tree", 't', Color.white, false, 0));
            configs.Add(CreateTileConfig(13, "Large Tree", 'T', Color.white, false, 0));
            
            // 建筑配置
            configs.Add(CreateTileConfig(14, "Bar", 'B', Color.white, false, 0));
            configs.Add(CreateTileConfig(15, "Yard", 'Y', Color.white, false, 0));
            configs.Add(CreateTileConfig(16, "Hotel", 'H', Color.white, false, 0));
            configs.Add(CreateTileConfig(17, "Exchange", 'E', Color.white, false, 0));
            
            // 玩家配置
            configs.Add(CreateTileConfig(-1, "Player", 'P', Color.white, true, 0));
            
            SaveConfig(config, configs);
        }
        
        private void GenerateConfigFromScene()
        {
            var config = ScriptableObject.CreateInstance<ASCIIConfig>();
            List<ASCIIConfig.TileConfig> configs = new List<ASCIIConfig.TileConfig>();
            
            // 从场景中提取配置
            ExtractTerrainConfigs(configs);
            ExtractBuildingConfigs(configs);
            ExtractSpecialPointConfigs(configs);
            
            SaveConfig(config, configs);
        }
        
        private void ExtractTerrainConfigs(List<ASCIIConfig.TileConfig> configs)
        {
            // 从TerrainType枚举提取地形配置
            foreach (TerrainType terrainType in System.Enum.GetValues(typeof(TerrainType)))
            {
                switch (terrainType)
                {
                    case TerrainType.Road:
                        configs.Add(CreateTileConfig(1, "Road", '.', Color.white, true, 1));
                        break;
                    case TerrainType.Grass:
                        configs.Add(CreateTileConfig(2, "Grass", '*', Color.white, true, 1));
                        break;
                    case TerrainType.Mountain:
                        configs.Add(CreateTileConfig(3, "Mountain", '^', Color.white, true, 3, 0.3f, 10));
                        break;
                    case TerrainType.Water:
                        configs.Add(CreateTileConfig(4, "Water", '~', Color.white, true, 2, 0.5f, 10));
                        break;
                    case TerrainType.CityWall:
                        configs.Add(CreateTileConfig(5, "City Wall", '#', Color.white, false, 0, isWall: true));
                        break;
                    case TerrainType.Tree:
                        configs.Add(CreateTileConfig(12, "Tree", 't', Color.white, false, 0));
                        break;
                    case TerrainType.LargeTree:
                        configs.Add(CreateTileConfig(13, "Large Tree", 'T', Color.white, false, 0));
                        break;
                }
            }
        }
        
        private void ExtractBuildingConfigs(List<ASCIIConfig.TileConfig> configs)
        {
            configs.Add(CreateTileConfig(14, "Bar", 'B', Color.white, false, 0));
            configs.Add(CreateTileConfig(15, "Yard", 'Y', Color.white, false, 0));
            configs.Add(CreateTileConfig(16, "Hotel", 'H', Color.white, false, 0));
            configs.Add(CreateTileConfig(17, "Exchange", 'E', Color.white, false, 0));
        }
        
        private void ExtractSpecialPointConfigs(List<ASCIIConfig.TileConfig> configs)
        {
            configs.Add(CreateTileConfig(9, "Task Point", '■', Color.yellow, true, 1, isSpecialPoint: true));
            configs.Add(CreateTileConfig(10, "Delivery Point", '□', Color.green, true, 1, isSpecialPoint: true));
            configs.Add(CreateTileConfig(11, "Rest Point", '+', Color.cyan, true, 1, isSpecialPoint: true));
        }
        
        private ASCIIConfig.TileConfig CreateTileConfig(
            int id, string name, char asciiChar, Color color, bool isPassable, int moveResistance,
            float strainChance = 0, int strainAmount = 0,
            bool isWall = false, bool isGate = false, bool isSpecialPoint = false)
        {
            return new ASCIIConfig.TileConfig
            {
                id = id,
                name = name,
                asciiChar = asciiChar,
                color = color,
                isPassable = isPassable,
                moveResistance = moveResistance,
                strainChance = strainChance,
                strainAmount = strainAmount,
                isWall = isWall,
                isGate = isGate,
                isSpecialPoint = isSpecialPoint
            };
        }
        
        private void SaveConfig(ASCIIConfig config, List<ASCIIConfig.TileConfig> configs)
        {
            try
            {
                // 确保输出路径存在
                if (!System.IO.Directory.Exists(outputPath))
                {
                    System.IO.Directory.CreateDirectory(outputPath);
                }
                
                // 设置配置
                config.tiles = configs.ToArray();
                
                // 保存资源
                string assetPath = $"{outputPath}/{configName}.asset";
                AssetDatabase.CreateAsset(config, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = config;
                
                Debug.Log($"ASCII配置已创建: {assetPath}，共 {configs.Count} 个配置项");
                EditorUtility.DisplayDialog("完成", $"ASCII配置已成功创建: {assetPath}", "确定");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"创建ASCII配置资源时出错: {e.Message}");
                EditorUtility.DisplayDialog("错误", $"创建ASCII配置资源时出错: {e.Message}", "确定");
            }
        }
    }
} 
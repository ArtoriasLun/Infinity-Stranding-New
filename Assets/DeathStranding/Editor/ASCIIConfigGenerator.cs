using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ALUNGAMES;
using System;
using System.Reflection;
using System.IO; // 确保有File IO的引用

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
        
        // 存储提取的配置数据
        private List<ASCIIConfig.TerrainConfig> terrainConfigs = new List<ASCIIConfig.TerrainConfig>();
        private List<ASCIIConfig.CityConfig> cityConfigs = new List<ASCIIConfig.CityConfig>();
        private List<ASCIIConfig.BuildingConfig> buildingConfigs = new List<ASCIIConfig.BuildingConfig>();
        private List<ASCIIConfig.CharacterConfig> characterConfigs = new List<ASCIIConfig.CharacterConfig>();
        private List<ASCIIConfig.ItemConfig> itemConfigs = new List<ASCIIConfig.ItemConfig>();
        
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
            
            EditorGUILayout.HelpBox("此工具将分析游戏管理器脚本并自动创建ASCIIConfig资源", MessageType.Info);
            EditorGUILayout.Space(10);
            
            // 配置选项
            configName = EditorGUILayout.TextField("配置名称", configName);
            outputPath = EditorGUILayout.TextField("输出路径", outputPath);
            
            EditorGUILayout.Space(10);
            
            // 引用对象字段
            buildingManager = (BuildingManager)EditorGUILayout.ObjectField("建筑管理器", buildingManager, typeof(BuildingManager), true);
            cityManager = (CityManager)EditorGUILayout.ObjectField("城市管理器", cityManager, typeof(CityManager), true);
            terrainGenerator = (TerrainGenerator)EditorGUILayout.ObjectField("地形生成器", terrainGenerator, typeof(TerrainGenerator), true);
            taskManager = (TaskManager)EditorGUILayout.ObjectField("任务管理器", taskManager, typeof(TaskManager), true);
            gameController = (GameController)EditorGUILayout.ObjectField("游戏控制器", gameController, typeof(GameController), true);
            
            EditorGUILayout.Space(10);
            
            // 在场景中查找对象的按钮
            if (GUILayout.Button("在场景中查找组件"))
            {
                FindComponentsInScene();
            }
            
            // 自动生成配置的按钮
            bool canGenerate = (buildingManager != null || cityManager != null || terrainGenerator != null);
            GUI.enabled = canGenerate;
            
            if (GUILayout.Button("生成ASCII配置"))
            {
                GenerateASCIIConfig();
            }
            
            GUI.enabled = true;
            
            if (!canGenerate)
            {
                EditorGUILayout.HelpBox("请至少提供一个管理器组件引用", MessageType.Warning);
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
        
        private void GenerateASCIIConfig()
        {
            // 清空之前的配置数据
            terrainConfigs.Clear();
            cityConfigs.Clear();
            buildingConfigs.Clear();
            characterConfigs.Clear();
            itemConfigs.Clear();
            
            // 从各个管理器获取数据
            ExtractTerrainData();
            ExtractBuildingData();
            ExtractCityData();
            ExtractItemsData();
            ExtractCharacterData();
            
            // 创建配置资源
            CreateASCIIConfigAsset();
        }
        
        private void ExtractTerrainData()
        {
            // 从TerrainType枚举和TerrainGenerator中提取地形配置
            Type terrainTypeEnum = typeof(TerrainType);
            foreach (TerrainType terrainType in Enum.GetValues(terrainTypeEnum))
            {
                ASCIIConfig.TerrainConfig config = new ASCIIConfig.TerrainConfig();
                config.terrainName = terrainType.ToString();
                
                // 设置默认ASCII字符
                switch (terrainType)
                {
                    case TerrainType.Grass:
                        config.asciiChar = '*';
                        break;
                    case TerrainType.Mountain:
                        config.asciiChar = '^';
                        break;
                    case TerrainType.Water:
                        config.asciiChar = '~';
                        break;
                    case TerrainType.Road:
                        config.asciiChar = '.';  // 修改为'.'表示道路
                        break;
                    case TerrainType.City:
                        config.asciiChar = '#';  // 墙壁，无法通行
                        break;
                    case TerrainType.Tree:
                        config.asciiChar = 'T';
                        break;
                    case TerrainType.LargeTree:
                        config.asciiChar = 'F';
                        break;
                    default:
                        config.asciiChar = '?';
                        break;
                }
                
                // 尝试从ASCIIRenderer中获取更多映射信息
                if (terrainGenerator != null)
                {
                    // 使用反射获取私有方法GetTerrainChar
                    FindTerrainCharMapping(terrainType, ref config);
                }
                
                terrainConfigs.Add(config);
            }
            
            Debug.Log($"提取到 {terrainConfigs.Count} 种地形配置");
        }
        
        private void FindTerrainCharMapping(TerrainType terrainType, ref ASCIIConfig.TerrainConfig config)
        {
            // 尝试从ASCIIRenderer中查找GetTerrainChar方法
            var asciiRenderer = FindObjectOfType<ASCIIRenderer>();
            if (asciiRenderer != null)
            {
                try
                {
                    MethodInfo method = typeof(ASCIIRenderer).GetMethod("GetTerrainChar", 
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    
                    if (method != null)
                    {
                        string result = (string)method.Invoke(asciiRenderer, new object[] { terrainType });
                        if (!string.IsNullOrEmpty(result) && result.Length > 0)
                        {
                            config.asciiChar = result[0];
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"获取地形字符映射时出错: {e.Message}");
                }
            }
        }
        
        private void ExtractBuildingData()
        {
            // 清除之前可能添加的建筑
            buildingConfigs.Clear();
            
            // 添加四个指定的建筑类型
            AddBuildingConfig("Bar", 'B');
            AddBuildingConfig("Yard", 'Y');
            AddBuildingConfig("Hotel", 'H');
            AddBuildingConfig("Exchange", 'E');
            
            Debug.Log($"提取了 {buildingConfigs.Count} 种建筑配置");
        }
        
        private void AddBuildingConfig(string name, char asciiChar)
        {
            ASCIIConfig.BuildingConfig config = new ASCIIConfig.BuildingConfig();
            config.buildingName = name;
            config.asciiChar = asciiChar;
            buildingConfigs.Add(config);
        }
        
        private void ExtractCityData()
        {
            // 清除之前可能添加的城市
            cityConfigs.Clear();
            
            if (cityManager == null)
            {
                Debug.LogWarning("CityManager为空，无法提取城市符号。将使用默认城市配置。");
                // 默认添加10个城市，使用数字作为字符
                for (int i = 0; i < 10; i++)
                {
                    ASCIIConfig.CityConfig cityConfig = new ASCIIConfig.CityConfig();
                    cityConfig.cityName = $"City_{i + 1}";
                    cityConfig.asciiChar = (i < 9) ? (char)('1' + i) : '0'; // 1-9, 然后是0
                    cityConfigs.Add(cityConfig);
                }
            }
            else
            {
            try
            {
                // 尝试获取citySymbolPool
                FieldInfo symbolPoolField = typeof(CityManager).GetField("citySymbolPool", 
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    
                if (symbolPoolField != null)
                {
                    string[] symbolPool = symbolPoolField.GetValue(cityManager) as string[];
                    
                    if (symbolPool != null)
                    {
                            for (int i = 0; i < symbolPool.Length && i < 10; i++) // 确保只提取10个城市
                        {
                            if (!string.IsNullOrEmpty(symbolPool[i]))
                            {
                                // 创建城市配置
                                ASCIIConfig.CityConfig cityConfig = new ASCIIConfig.CityConfig();
                                cityConfig.cityName = $"City_{i + 1}";
                                cityConfig.asciiChar = symbolPool[i][0];
                                cityConfigs.Add(cityConfig);
                                }
                                else
                                {
                                    // 如果符号池中的某个元素为空，使用默认符号
                                    ASCIIConfig.CityConfig cityConfig = new ASCIIConfig.CityConfig();
                                    cityConfig.cityName = $"City_{i + 1}";
                                    cityConfig.asciiChar = (i < 9) ? (char)('1' + i) : '0'; // 1-9, 然后是0
                                    cityConfigs.Add(cityConfig);
                                }
                            }
                        }
                        else
                        {
                            // 如果符号池为null，使用默认配置
                            CreateDefaultCities();
                        }
                    }
                    else
                    {
                        // 如果没有找到符号池字段，使用默认配置
                        CreateDefaultCities();
                    }
                    
                    // 确保有10个城市
                    EnsureTenCities();
                }
                catch (Exception e)
                {
                    Debug.LogError($"提取城市数据时出错: {e.Message}");
                    CreateDefaultCities();
                    EnsureTenCities();
                }
            }
            
            Debug.Log($"提取了 {cityConfigs.Count} 种城市配置");
        }
        
        private void CreateDefaultCities()
        {
            // 创建默认的10个城市配置
            for (int i = 0; i < 10; i++)
            {
                ASCIIConfig.CityConfig cityConfig = new ASCIIConfig.CityConfig();
                cityConfig.cityName = $"City_{i + 1}";
                cityConfig.asciiChar = (i < 9) ? (char)('1' + i) : '0'; // 1-9, 然后是0
                cityConfigs.Add(cityConfig);
            }
        }
        
        private void EnsureTenCities()
        {
            // 确保有10个城市
            int currentCount = cityConfigs.Count;
            if (currentCount < 10)
            {
                for (int i = currentCount; i < 10; i++)
                {
                    ASCIIConfig.CityConfig cityConfig = new ASCIIConfig.CityConfig();
                    cityConfig.cityName = $"City_{i + 1}";
                    cityConfig.asciiChar = (i < 9) ? (char)('1' + i) : '0'; // 1-9, 然后是0
                    cityConfigs.Add(cityConfig);
                }
            }
            else if (currentCount > 10)
            {
                // 如果有超过10个，只保留前10个
                cityConfigs = cityConfigs.GetRange(0, 10);
            }
        }
        
        private void ExtractItemsData()
        {
            // 清除之前可能添加的物品
            itemConfigs.Clear();
            
            // 尝试从BuildingManager获取特殊字符
            Dictionary<string, char> specialChars = new Dictionary<string, char>
            {
                { "Task", 'T' },
                { "Delivery", 'D' },
                { "Rest", 'R' }
            };
            
            if (buildingManager != null)
            {
            try
            {
                // 获取specialPoints字典
                FieldInfo specialPointsField = typeof(BuildingManager).GetField("specialPoints", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                    
                if (specialPointsField != null)
                {
                    var specialPoints = specialPointsField.GetValue(buildingManager) as Dictionary<string, string>;
                    
                    if (specialPoints != null)
                    {
                            // 查找任务、递送和休息点的字符
                        foreach (var entry in specialPoints)
                            {
                                string itemKey = entry.Key.ToLower();
                                if (itemKey.Contains("task") && !string.IsNullOrEmpty(entry.Value))
                                {
                                    specialChars["Task"] = entry.Value[0];
                                }
                                else if (itemKey.Contains("delivery") && !string.IsNullOrEmpty(entry.Value))
                                {
                                    specialChars["Delivery"] = entry.Value[0];
                                }
                                else if (itemKey.Contains("rest") && !string.IsNullOrEmpty(entry.Value))
                                {
                                    specialChars["Rest"] = entry.Value[0];
                                }
                            }
                        }
                    }
            }
            catch (Exception e)
            {
                    Debug.LogError($"从BuildingManager获取特殊点数据时出错: {e.Message}");
                }
            }
            
            // 添加三个指定的物品类型，使用从BuildingManager获取的实际字符
            AddItemConfig("Task", specialChars["Task"]);
            AddItemConfig("Delivery", specialChars["Delivery"]);
            AddItemConfig("Rest", specialChars["Rest"]);
            
            Debug.Log($"提取了 {itemConfigs.Count} 个物品配置: " +
                $"Task({specialChars["Task"]}), Delivery({specialChars["Delivery"]}), Rest({specialChars["Rest"]})");
        }
        
        private void AddItemConfig(string name, char asciiChar)
        {
            ASCIIConfig.ItemConfig config = new ASCIIConfig.ItemConfig();
            config.itemName = name;
            config.asciiChar = asciiChar;
            itemConfigs.Add(config);
        }
        
        private void ExtractCharacterData()
        {
            // 只添加玩家角色
            ASCIIConfig.CharacterConfig playerConfig = new ASCIIConfig.CharacterConfig();
            playerConfig.characterName = "Player";
            playerConfig.asciiChar = '@'; // 将字符设置为'@'
            characterConfigs.Add(playerConfig);
            
            Debug.Log("提取了1个角色配置");
        }
        
        private void CreateASCIIConfigAsset()
        {
            try
            {
                // 确保输出路径存在
                if (!System.IO.Directory.Exists(outputPath))
                {
                    System.IO.Directory.CreateDirectory(outputPath);
                }
                
                // 创建配置资源
                ASCIIConfig config = ScriptableObject.CreateInstance<ASCIIConfig>();
                
                // 设置提取的数据
                config.terrains = terrainConfigs.ToArray();
                config.cities = cityConfigs.ToArray();
                config.buildings = buildingConfigs.ToArray();
                config.characters = characterConfigs.ToArray();
                config.items = itemConfigs.ToArray();
                
                // 始终创建字符精灵
                CreateCharacterSprites(config);
                
                // 保存资源
                string assetPath = $"{outputPath}/{configName}.asset";
                AssetDatabase.CreateAsset(config, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = config;
                
                Debug.Log($"ASCII配置已创建: {assetPath}");
                EditorUtility.DisplayDialog("完成", $"ASCII配置已成功创建: {assetPath}", "确定");
            }
            catch (Exception e)
            {
                Debug.LogError($"创建ASCII配置资源时出错: {e.Message}");
                EditorUtility.DisplayDialog("错误", $"创建ASCII配置资源时出错: {e.Message}", "确定");
            }
        }
        
        private void CreateCharacterSprites(ASCIIConfig config)
        {
            // 为地形创建字符精灵
            for (int i = 0; i < config.terrains.Length; i++)
            {
                var terrain = config.terrains[i];
                Color textColor = GetColorForTerrainType(terrain.terrainName);
                terrain.sprite = CreateSpriteFromChar(terrain.terrainName, terrain.asciiChar, textColor);
                config.terrains[i] = terrain;
            }
            
            // 为城市创建字符精灵
            for (int i = 0; i < config.cities.Length; i++)
            {
                var city = config.cities[i];
                Color textColor = new Color(0.8f, 0.4f, 0.4f); // 城市使用红色调
                city.sprite = CreateSpriteFromChar(city.cityName, city.asciiChar, textColor);
                config.cities[i] = city;
            }
            
            // 为建筑创建字符精灵
            for (int i = 0; i < config.buildings.Length; i++)
            {
                var building = config.buildings[i];
                Color textColor = new Color(0.6f, 0.6f, 0.4f); // 建筑使用褐色调
                building.sprite = CreateSpriteFromChar(building.buildingName, building.asciiChar, textColor);
                config.buildings[i] = building;
            }
            
            // 为角色创建字符精灵
            for (int i = 0; i < config.characters.Length; i++)
            {
                var character = config.characters[i];
                Color textColor = Color.yellow; // 角色使用黄色
                character.sprite = CreateSpriteFromChar(character.characterName, character.asciiChar, textColor);
                config.characters[i] = character;
            }
            
            // 为物品创建字符精灵
            for (int i = 0; i < config.items.Length; i++)
            {
                var item = config.items[i];
                Color textColor = Color.white; // 物品使用白色
                item.sprite = CreateSpriteFromChar(item.itemName, item.asciiChar, textColor);
                config.items[i] = item;
            }
        }
        
        private Sprite CreateSpriteFromChar(string name, char character, Color textColor)
        {
            // 确保存在精灵文件夹
            string spriteFolderPath = $"{outputPath}/Sprites";
            if (!System.IO.Directory.Exists(spriteFolderPath))
            {
                System.IO.Directory.CreateDirectory(spriteFolderPath);
            }
            
            // 背景色统一为黑色
            Color backgroundColor = Color.black;
            
            int imageSize = 32; // 精灵尺寸，可以根据需要调整
            
            // 生成纹理
            Texture2D texture = GenerateCharacterTexture(
                character,
                imageSize,
                backgroundColor,
                textColor, // 文字颜色使用传入的颜色
                null, // 使用默认字体
                Mathf.RoundToInt(imageSize * 0.8f) // 字体大小为图像大小的80%
            );
            
            // 创建精灵
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 32);
            
            // 保存纹理和精灵资源
            string texturePath = $"{spriteFolderPath}/{name}_Texture.asset";
            string spritePath = $"{spriteFolderPath}/{name}_Sprite.asset";
            
            AssetDatabase.CreateAsset(texture, texturePath);
            AssetDatabase.CreateAsset(sprite, spritePath);
            
            return sprite;
        }
        
        // 直接集成字符图片生成功能
        private Texture2D GenerateCharacterTexture(char character, int imageSize = 32, Color? backgroundColor = null, 
                                                  Color? foregroundColor = null, Font font = null, int fontSize = 24)
        {
            // 设置默认值
            backgroundColor = backgroundColor ?? Color.white;
            foregroundColor = foregroundColor ?? Color.black;
            
            if (font == null)
            {
                font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
            }
            
            // 创建纹理
            Texture2D texture = new Texture2D(imageSize, imageSize, TextureFormat.RGBA32, false);
            
            // 填充背景色
            Color[] colors = new Color[imageSize * imageSize];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = backgroundColor.Value;
            }
            texture.SetPixels(colors);
            texture.Apply();
            
            // 绘制字符
            DrawCharacterOnTexture(texture, character.ToString(), foregroundColor.Value, font, fontSize, backgroundColor.Value);
            
            return texture;
        }
        
        private void DrawCharacterOnTexture(Texture2D texture, string text, Color textColor, Font font, int fontSize, Color backgroundColor)
        {
            // 创建临时的TextMesh对象
            GameObject tempObj = new GameObject("TempTextRenderer");
            TextMesh textMesh = tempObj.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.fontSize = 100; // 固定字体大小，后面会进行缩放
            textMesh.color = textColor;
            textMesh.font = font;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.characterSize = 0.5f;
            
            // 添加Mesh渲染器组件并设置
            MeshRenderer renderer = tempObj.GetComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("GUI/Text Shader"));
            renderer.material.mainTexture = font.material.mainTexture;
            
            // 创建容器对象
            GameObject container = new GameObject("TextContainer");
            tempObj.transform.parent = container.transform;
            
            // 创建临时相机
            GameObject cameraObj = new GameObject("TempCamera");
            Camera camera = cameraObj.AddComponent<Camera>();
            camera.backgroundColor = backgroundColor;
            camera.clearFlags = CameraClearFlags.SolidColor;  // 不使用透明
            camera.orthographic = true;
            camera.orthographicSize = 0.6f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 10f;
            
            // 放置相机和文本
            container.transform.position = Vector3.zero;
            cameraObj.transform.position = new Vector3(0, 0, -5);
            cameraObj.transform.LookAt(container.transform.position);
            
            // 缩放调整
            float scaleFactor = fontSize / 100.0f;
            tempObj.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            
            // 创建RenderTexture
            RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = renderTexture;
            camera.Render();
            
            // 复制渲染纹理到目标纹理
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
            
            // 清理
            RenderTexture.active = null;
            camera.targetTexture = null;
            RenderTexture.ReleaseTemporary(renderTexture);
            UnityEngine.Object.DestroyImmediate(container);
            UnityEngine.Object.DestroyImmediate(cameraObj);
        }
        
        private Color GetColorForTerrainType(string terrainName)
        {
            terrainName = terrainName.ToLower();
            
            if (terrainName.Contains("water"))
                return new Color(0, 0.7f, 1f);
            
            if (terrainName.Contains("mountain"))
                return new Color(0.7f, 0.7f, 0.7f);
            
            if (terrainName.Contains("grass"))
                return new Color(0.2f, 0.8f, 0.2f);
            
            if (terrainName.Contains("road"))
                return new Color(0.8f, 0.8f, 0.5f);
            
            if (terrainName.Contains("city"))
                return new Color(0.8f, 0.5f, 0.5f);
            
            if (terrainName.Contains("tree"))
                return new Color(0.1f, 0.6f, 0.1f);
            
            return new Color(0.5f, 0.5f, 0.5f);
        }
    }
} 
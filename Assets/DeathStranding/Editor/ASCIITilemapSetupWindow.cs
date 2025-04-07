using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;

namespace ALUNGAMES.Editor
{
    public class ASCIITilemapSetupWindow : EditorWindow
    {
        private GameObject targetASCIIRenderer;
        private ASCIIConfig asciiConfig;
        private string rootObjectName = "ASCII_Tilemap";
        private bool createDefaultTiles = true;

        [MenuItem("😊/ASCII Tilemap Setup")]
        public static void ShowWindow()
        {
            GetWindow<ASCIITilemapSetupWindow>("ASCII Tilemap Setup");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("ASCII Tilemap Setup Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("此工具将创建Tilemap结构，并更新ASCIIRenderer以使用Tilemap渲染游戏地图", MessageType.Info);
            EditorGUILayout.Space(10);

            targetASCIIRenderer = (GameObject)EditorGUILayout.ObjectField("ASCII渲染器对象", targetASCIIRenderer, typeof(GameObject), true);
            asciiConfig = (ASCIIConfig)EditorGUILayout.ObjectField("ASCII配置", asciiConfig, typeof(ASCIIConfig), false);
            rootObjectName = EditorGUILayout.TextField("Tilemap根对象名称", rootObjectName);
            createDefaultTiles = EditorGUILayout.Toggle("创建默认Tiles", createDefaultTiles);

            EditorGUILayout.Space(10);
            GUI.enabled = targetASCIIRenderer != null;
            if (GUILayout.Button("创建Tilemap结构"))
            {
                CreateTilemapStructure();
            }
            GUI.enabled = true;
            
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("提示: 确保您已安装Unity 2D Tilemap包\n(Window > Package Manager > Unity Registry > 2D Tilemap Editor)", MessageType.Info);
        }

        private void CreateTilemapStructure()
        {
            if (targetASCIIRenderer == null)
            {
                EditorUtility.DisplayDialog("错误", "请选择包含ASCIIRenderer的游戏对象", "确定");
                return;
            }

            // 检查ASCIIRenderer组件
            ALUNGAMES.ASCIIRenderer renderer = targetASCIIRenderer.GetComponent<ALUNGAMES.ASCIIRenderer>();
            if (renderer == null)
            {
                EditorUtility.DisplayDialog("错误", "选择的对象没有ASCIIRenderer组件", "确定");
                return;
            }

            // 创建tilemap根对象
            GameObject tilemapRoot = new GameObject(rootObjectName);
            Undo.RegisterCreatedObjectUndo(tilemapRoot, "Create Tilemap Root");
            
            // 添加Grid组件
            Grid grid = tilemapRoot.AddComponent<Grid>();
            grid.cellSize = new Vector3(1, 1, 0);

            // 创建不同层的Tilemap
            Tilemap terrainTilemap = CreateTilemapLayer(tilemapRoot, "TerrainLayer", 0);
            Tilemap objectsTilemap = CreateTilemapLayer(tilemapRoot, "ObjectsLayer", 1);
            Tilemap characterTilemap = CreateTilemapLayer(tilemapRoot, "CharacterLayer", 2);

            // 修改ASCIIRenderer脚本
            UpdateRendererScript();

            // 等待编译完成
            AssetDatabase.Refresh();
            EditorApplication.delayCall += () =>
            {
                // 重新获取组件引用（编译后可能已经更改）
                renderer = targetASCIIRenderer.GetComponent<ALUNGAMES.ASCIIRenderer>();
                if (renderer == null)
                {
                    Debug.LogError("编译后无法找到ASCIIRenderer组件");
                    return;
                }

                // 设置新字段的引用
                SerializedObject serializedRenderer = new SerializedObject(renderer);

                // 设置Grid引用
                SerializedProperty gridProp = serializedRenderer.FindProperty("grid");
                if (gridProp != null) gridProp.objectReferenceValue = grid;

                // 设置Tilemaps引用
                SerializedProperty terrainTilemapProp = serializedRenderer.FindProperty("terrainTilemap");
                if (terrainTilemapProp != null) terrainTilemapProp.objectReferenceValue = terrainTilemap;

                SerializedProperty objectsTilemapProp = serializedRenderer.FindProperty("objectsTilemap");
                if (objectsTilemapProp != null) objectsTilemapProp.objectReferenceValue = objectsTilemap;

                SerializedProperty characterTilemapProp = serializedRenderer.FindProperty("characterTilemap");
                if (characterTilemapProp != null) characterTilemapProp.objectReferenceValue = characterTilemap;

                // 设置ASCIIConfig
                if (asciiConfig != null)
                {
                    SerializedProperty configProp = serializedRenderer.FindProperty("asciiConfig");
                    if (configProp != null) configProp.objectReferenceValue = asciiConfig;
                }

                serializedRenderer.ApplyModifiedProperties();

                // 选中新创建的根对象
                Selection.activeGameObject = tilemapRoot;
                
                if (createDefaultTiles && asciiConfig != null)
                {
                    CreateDefaultTiles();
                }

                Debug.Log("Tilemap结构创建成功！");
            };
        }

        private Tilemap CreateTilemapLayer(GameObject parent, string name, int sortingOrder)
        {
            GameObject tilemapObject = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(tilemapObject, "Create Tilemap Layer");
            tilemapObject.transform.SetParent(parent.transform, false);
            
            Tilemap tilemap = tilemapObject.AddComponent<Tilemap>();
            TilemapRenderer renderer = tilemapObject.AddComponent<TilemapRenderer>();
            
            renderer.sortingOrder = sortingOrder;
            
            return tilemap;
        }

        private void UpdateRendererScript()
        {
            // 获取ASCIIRenderer脚本路径
            string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(targetASCIIRenderer.GetComponent<ALUNGAMES.ASCIIRenderer>()));
            
            if (string.IsNullOrEmpty(scriptPath))
            {
                EditorUtility.DisplayDialog("错误", "无法获取ASCIIRenderer脚本路径", "确定");
                return;
            }

            // 读取脚本内容
            string scriptContent = File.ReadAllText(scriptPath);
            
            // 跳过更新如果已经包含Tilemap
            if (scriptContent.Contains("UnityEngine.Tilemaps"))
            {
                Debug.Log("ASCIIRenderer已经包含Tilemap引用，跳过更新");
                return;
            }

            // 修改脚本添加Tilemap支持
            string updatedScript = scriptContent;
            
            // 添加using语句
            if (!updatedScript.Contains("using UnityEngine.Tilemaps;"))
            {
                updatedScript = updatedScript.Replace("using UnityEngine;", 
                    "using UnityEngine;\nusing UnityEngine.Tilemaps;");
            }
            
            // 添加字段
            string fieldsToAdd = @"
        [Header(""Tilemap References"")]
        [SerializeField] private Grid grid;
        [SerializeField] private Tilemap terrainTilemap;
        [SerializeField] private Tilemap objectsTilemap;
        [SerializeField] private Tilemap characterTilemap;
        [SerializeField] private ASCIIConfig asciiConfig;
        
        // Tile字典
        private Dictionary<char, TileBase> terrainTiles = new Dictionary<char, TileBase>();
        private Dictionary<char, TileBase> objectTiles = new Dictionary<char, TileBase>();
        private Dictionary<char, TileBase> characterTiles = new Dictionary<char, TileBase>();
";
            
            // 查找合适的插入点 - 在私有字段定义后
            int insertIndex = updatedScript.IndexOf("private VisualElement root;");
            if (insertIndex > 0)
            {
                // 找到这一行的末尾
                int endOfLine = updatedScript.IndexOf('\n', insertIndex);
                if (endOfLine > 0)
                {
                    updatedScript = updatedScript.Insert(endOfLine + 1, fieldsToAdd);
                }
            }
            
            // 添加初始化Tile字典的方法
            string initTilesMethod = @"
        // 初始化Tile字典
        private void InitializeTileDictionaries()
        {
            if (asciiConfig == null) 
            {
                Debug.LogError(""ASCIIConfig为null，无法初始化Tile字典"");
                return;
            }
            
            // 初始化地形Tiles
            foreach (var terrain in asciiConfig.terrains)
            {
                if (terrain.sprite != null)
                {
                    var tile = CreateTileFromSprite(terrain.sprite);
                    terrainTiles[terrain.asciiChar] = tile;
                }
            }
            
            // 初始化角色Tiles
            foreach (var character in asciiConfig.characters)
            {
                if (character.sprite != null)
                {
                    var tile = CreateTileFromSprite(character.sprite);
                    characterTiles[character.asciiChar] = tile;
                }
            }
            
            // 初始化物品Tiles
            foreach (var item in asciiConfig.items)
            {
                if (item.sprite != null)
                {
                    var tile = CreateTileFromSprite(item.sprite);
                    objectTiles[item.asciiChar] = tile;
                }
            }
            
            // 添加默认玩家Tile，如果配置中没有
            if (!characterTiles.ContainsKey('@'))
            {
                var tile = CreateDefaultTile(Color.yellow);
                characterTiles['@'] = tile;
            }
        }
        
        // 创建Tile的辅助方法
        private TileBase CreateTileFromSprite(Sprite sprite)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            return tile;
        }
        
        // 创建默认颜色Tile
        private TileBase CreateDefaultTile(Color color)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            // 创建一个纯色精灵
            Texture2D texture = new Texture2D(32, 32);
            for(int y = 0; y < texture.height; y++)
            {
                for(int x = 0; x < texture.width; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply();
            
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 32);
            tile.sprite = sprite;
            
            return tile;
        }";
            
            // 查找合适的插入点 - 在类的末尾
            int classEndIndex = updatedScript.LastIndexOf("}");
            if (classEndIndex > 0)
            {
                // 找到上一个方法的结束位置
                int previousMethodEnd = updatedScript.LastIndexOf("}", classEndIndex - 1);
                if (previousMethodEnd > 0)
                {
                    updatedScript = updatedScript.Insert(previousMethodEnd + 1, initTilesMethod);
                }
            }
            
            // 修改Awake方法
            string awakeMethod = @"
        private void Awake()
        {
            // 初始化Tile字典
            InitializeTileDictionaries();
        }";
            
            // 检查是否已有Awake方法
            if (!updatedScript.Contains("void Awake()"))
            {
                // 在OnEnable前添加Awake
                int onEnableIndex = updatedScript.IndexOf("void OnEnable()");
                if (onEnableIndex > 0)
                {
                    updatedScript = updatedScript.Insert(onEnableIndex, awakeMethod);
                }
            }
            
            // 修改RenderMap方法使用Tilemap
            string oldRenderMap = "public void RenderMap()";
            int renderMapIndex = updatedScript.IndexOf(oldRenderMap);
            
            if (renderMapIndex > 0)
            {
                // 找到方法体的开始和结束
                int methodStart = updatedScript.IndexOf("{", renderMapIndex);
                int methodEnd = -1;
                
                if (methodStart > 0)
                {
                    // 找到匹配的闭括号
                    int braceCount = 1;
                    for (int i = methodStart + 1; i < updatedScript.Length; i++)
                    {
                        if (updatedScript[i] == '{') braceCount++;
                        else if (updatedScript[i] == '}') braceCount--;
                        
                        if (braceCount == 0)
                        {
                            methodEnd = i;
                            break;
                        }
                    }
                }
                
                if (methodEnd > 0)
                {
                    string newRenderMapMethod = @"public void RenderMap()
        {
            try
            {
                // 如果Tilemap可用，使用Tilemap渲染
                if (terrainTilemap != null && characterTilemap != null)
                {
                    RenderMapWithTilemap();
                    return;
                }
                
                // 否则使用原始UI方式渲染
                RenderMapWithUI();
            }
            catch (System.Exception e)
            {
                Debug.LogError($""渲染地图时发生错误: {e.Message}\n{e.StackTrace}"");
            }
        }
        
        // 使用Tilemap渲染地图
        private void RenderMapWithTilemap()
        {
            // 清空所有Tilemap
            terrainTilemap.ClearAllTiles();
            objectsTilemap.ClearAllTiles();
            characterTilemap.ClearAllTiles();
            
            // 获取当前地形
            TerrainType[,] terrain = GameController.Instance.GetCurrentTerrain();
            if (terrain == null)
            {
                Debug.LogError(""RenderMapWithTilemap: 地形为null"");
                return;
            }
            
            // 获取实际地形大小
            int terrainSizeY = terrain.GetLength(0);
            int terrainSizeX = terrain.GetLength(1);
            
            // 确保地形尺寸有效
            if (terrainSizeY <= 0 || terrainSizeX <= 0)
            {
                Debug.LogError($""RenderMapWithTilemap: 无效的地形尺寸: {terrainSizeY}x{terrainSizeX}"");
                return;
            }
            
            // 在Tilemap上渲染地形
            for (int y = 0; y < terrainSizeY; y++)
            {
                for (int x = 0; x < terrainSizeX; x++)
                {
                    // 获取地形字符
                    char terrainChar = GetTerrainChar(terrain[y, x])[0];
                    
                    // 根据字符获取对应的Tile
                    if (terrainTiles.TryGetValue(terrainChar, out TileBase tile))
                    {
                        // 设置Tile到地形Tilemap
                        terrainTilemap.SetTile(new Vector3Int(x, y, 0), tile);
                    }
                }
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
                    // 获取玩家Tile
                    if (characterTiles.TryGetValue('@', out TileBase playerTile))
                    {
                        characterTilemap.SetTile(new Vector3Int(playerX, playerY, 0), playerTile);
                    }
                }
            }
            
            // 如果UI游戏地图存在，显示提示信息
            if (gameMap != null)
            {
                gameMap.Clear();
                var infoLabel = new Label(""游戏使用Tilemap渲染中"");
                infoLabel.style.color = Color.white;
                infoLabel.style.fontSize = 14;
                infoLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                gameMap.Add(infoLabel);
            }
        }
        
        // 原始UI渲染方法
        private void RenderMapWithUI()";
                    
                    // 替换整个方法
                    updatedScript = updatedScript.Remove(renderMapIndex, methodEnd - renderMapIndex + 1);
                    updatedScript = updatedScript.Insert(renderMapIndex, newRenderMapMethod);
                }
            }
            
            // 修改UpdatePlayerPositionOnMap方法
            string oldUpdateMethod = "public void UpdatePlayerPositionOnMap()";
            int updateMethodIndex = updatedScript.IndexOf(oldUpdateMethod);
            
            if (updateMethodIndex > 0)
            {
                // 找到方法体的开始和结束
                int methodStart = updatedScript.IndexOf("{", updateMethodIndex);
                int methodEnd = -1;
                
                if (methodStart > 0)
                {
                    // 找到匹配的闭括号
                    int braceCount = 1;
                    for (int i = methodStart + 1; i < updatedScript.Length; i++)
                    {
                        if (updatedScript[i] == '{') braceCount++;
                        else if (updatedScript[i] == '}') braceCount--;
                        
                        if (braceCount == 0)
                        {
                            methodEnd = i;
                            break;
                        }
                    }
                }
                
                if (methodEnd > 0)
                {
                    string newUpdateMethod = @"public void UpdatePlayerPositionOnMap()
        {
            // 如果Tilemap可用，使用Tilemap更新玩家位置
            if (characterTilemap != null)
            {
                UpdatePlayerPositionOnTilemap();
                return;
            }
            
            // 否则使用原始UI方式更新
            UpdatePlayerPositionOnUI();
        }
        
        // 使用Tilemap更新玩家位置
        private void UpdatePlayerPositionOnTilemap()
        {
            var playerController = GameController.Instance.PlayerController;
            if (playerController == null || characterTilemap == null)
                return;
            
            try {
                // 清除角色Tilemap
                characterTilemap.ClearAllTiles();
                
                // 获取地形信息用于边界检查
                TerrainType[,] terrain = GameController.Instance.GetCurrentTerrain();
                if (terrain == null) return;
                
                int terrainSizeY = terrain.GetLength(0);
                int terrainSizeX = terrain.GetLength(1);
                
                // 获取玩家位置
                Vector2Int playerPos = playerController.PlayerPosition;
                
                // 边界检查
                if (playerPos.x >= 0 && playerPos.x < terrainSizeX && 
                    playerPos.y >= 0 && playerPos.y < terrainSizeY)
                {
                    // 在Tilemap上设置玩家位置
                    if (characterTiles.TryGetValue('@', out TileBase playerTile))
                    {
                        characterTilemap.SetTile(
                            new Vector3Int(playerPos.x, playerPos.y, 0), 
                            playerTile);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($""更新玩家Tilemap位置时出错: {e.Message}"");
            }
        }
        
        // 原始UI更新方法
        private void UpdatePlayerPositionOnUI()";
                    
                    // 替换整个方法
                    updatedScript = updatedScript.Remove(updateMethodIndex, methodEnd - updateMethodIndex + 1);
                    updatedScript = updatedScript.Insert(updateMethodIndex, newUpdateMethod);
                }
            }

            // 保存修改后的脚本
            File.WriteAllText(scriptPath, updatedScript);
            
            AssetDatabase.Refresh();
            Debug.Log("ASCIIRenderer脚本已更新，添加了Tilemap支持");
        }

        private void CreateDefaultTiles()
        {
            if (asciiConfig == null) return;
            
            string folderPath = "Assets/DeathStranding/Tiles";
            
            // 确保文件夹存在
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parentFolder = Path.GetDirectoryName(folderPath);
                string newFolderName = Path.GetFileName(folderPath);
                AssetDatabase.CreateFolder(parentFolder, newFolderName);
            }
            
            // 为每种地形创建默认瓦片
            foreach (var terrain in asciiConfig.terrains)
            {
                string tileAssetPath = $"{folderPath}/Terrain_{terrain.terrainName}.asset";
                
                // 检查是否已存在
                if (AssetDatabase.LoadAssetAtPath<Tile>(tileAssetPath) == null)
                {
                    Tile tile = ScriptableObject.CreateInstance<Tile>();
                    
                    // 如果有sprite使用它，否则创建默认sprite
                    if (terrain.sprite != null)
                    {
                        tile.sprite = terrain.sprite;
                    }
                    else
                    {
                        Color tileColor = GetColorForTerrainType(terrain.terrainName);
                        Texture2D texture = CreateColorTexture(32, 32, tileColor);
                        
                        // 在纹理上绘制ASCII字符
                        DrawCharacterOnTexture(texture, terrain.asciiChar.ToString(), Color.black);
                        
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 32);
                        tile.sprite = sprite;
                    }
                    
                    AssetDatabase.CreateAsset(tile, tileAssetPath);
                }
            }
            
            // 为角色创建瓦片...
            foreach (var character in asciiConfig.characters)
            {
                string tileAssetPath = $"{folderPath}/Character_{character.characterName}.asset";
                
                if (AssetDatabase.LoadAssetAtPath<Tile>(tileAssetPath) == null)
                {
                    Tile tile = ScriptableObject.CreateInstance<Tile>();
                    
                    if (character.sprite != null)
                    {
                        tile.sprite = character.sprite;
                    }
                    else
                    {
                        Texture2D texture = CreateColorTexture(32, 32, new Color(1f, 0.8f, 0.2f));
                        DrawCharacterOnTexture(texture, character.asciiChar.ToString(), Color.black);
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 32);
                        tile.sprite = sprite;
                    }
                    
                    AssetDatabase.CreateAsset(tile, tileAssetPath);
                }
            }
            
            // 保存默认玩家瓦片
            string playerTilePath = $"{folderPath}/Character_Player.asset";
            if (AssetDatabase.LoadAssetAtPath<Tile>(playerTilePath) == null &&
                !System.Array.Exists(asciiConfig.characters, c => c.asciiChar == '@'))
            {
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                Texture2D texture = CreateColorTexture(32, 32, Color.yellow);
                DrawCharacterOnTexture(texture, "@", Color.black);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 32);
                tile.sprite = sprite;
                
                AssetDatabase.CreateAsset(tile, playerTilePath);
            }
            
            // 刷新资源数据库
            AssetDatabase.Refresh();
            Debug.Log("已创建默认瓦片资产");
        }
        
        private Texture2D CreateColorTexture(int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];
            
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        private void DrawCharacterOnTexture(Texture2D texture, string character, Color textColor)
        {
            // 此处简化处理：在纹理中央绘制一个点代表字符
            // 实际项目中可能需要更复杂的文本渲染
            int centerX = texture.width / 2;
            int centerY = texture.height / 2;
            
            // 创建一个简单的点阵
            for (int y = -3; y <= 3; y++)
            {
                for (int x = -3; x <= 3; x++)
                {
                    if (centerX + x >= 0 && centerX + x < texture.width &&
                        centerY + y >= 0 && centerY + y < texture.height)
                    {
                        texture.SetPixel(centerX + x, centerY + y, textColor);
                    }
                }
            }
            
            texture.Apply();
        }
        
        private Color GetColorForTerrainType(string terrainName)
        {
            terrainName = terrainName.ToLower();
            
            if (terrainName.Contains("water") || terrainName.Contains("sea") || terrainName.Contains("ocean"))
                return new Color(0, 0.7f, 1f);
            
            if (terrainName.Contains("mountain") || terrainName.Contains("hill"))
                return new Color(0.7f, 0.7f, 0.7f);
            
            if (terrainName.Contains("grass") || terrainName.Contains("plain"))
                return new Color(0.2f, 0.8f, 0.2f);
            
            if (terrainName.Contains("road") || terrainName.Contains("path"))
                return new Color(0.8f, 0.8f, 0.5f);
            
            if (terrainName.Contains("city") || terrainName.Contains("building"))
                return new Color(0.8f, 0.5f, 0.5f);
            
            if (terrainName.Contains("tree") || terrainName.Contains("forest"))
                return new Color(0.1f, 0.6f, 0.1f);
            
            // 默认颜色
            return new Color(0.5f, 0.5f, 0.5f);
        }
    }
}

using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ALUNGAMES
{
    /// <summary>
    /// 使用Tilemap渲染ASCII游戏地图的组件
    /// </summary>
    public class ASCIITilemapRenderer : MonoBehaviour
    {
        [Header("配置")]
        [SerializeField] private ASCIIConfig asciiConfig;
        
        [Header("Tilemap引用")]
        [SerializeField] private Grid grid;
        [SerializeField] private Tilemap terrainTilemap;
        [SerializeField] private Tilemap objectsTilemap;
        [SerializeField] private Tilemap characterTilemap;
        
        [Header("ASCIIRenderer引用")]
        [SerializeField] private ASCIIRenderer asciiRenderer;
        
        [Header("设置")]
        [SerializeField] private bool createTilemapsAutomatically = true;
        [SerializeField] private float tileSize = 1.0f;
        [SerializeField] private bool debugMode = true;
        
        // Tile字典
        private Dictionary<char, TileBase> terrainTiles = new Dictionary<char, TileBase>();
        private Dictionary<char, TileBase> objectTiles = new Dictionary<char, TileBase>();
        private Dictionary<char, TileBase> characterTiles = new Dictionary<char, TileBase>();
        
        // UI元素
        private VisualElement root;
        private VisualElement gameMap;
        
        // 公共事件
        public System.Action OnMapRendered;
        
        private void Awake()
        {
            Debug.Log("ASCIITilemapRenderer: Awake");
            
            if (createTilemapsAutomatically)
                CreateTilemapHierarchy();
                
            InitializeTileDictionaries();
            
            // 初始化UI
            InitializeUI();
        }
        
        private void Start()
        {
            Debug.Log("ASCIITilemapRenderer: Start");
            
            // 确保GameController已经初始化
            if (GameController.Instance == null || GameController.Instance.PlayerController == null)
            {
                Debug.LogError("ASCIITilemapRenderer: GameController或PlayerController未初始化");
                return;
            }
            
            // 直接从GameController获取ASCIIRenderer (如果没有设置)
            if (asciiRenderer == null)
            {
                asciiRenderer = GameController.Instance.ASCIIRenderer;
                if (asciiRenderer == null)
                    Debug.LogError("ASCIITilemapRenderer: 从GameController无法获取ASCIIRenderer组件，无法同步地图数据");
            }
            
            // 等待ASCIIRenderer渲染完成后同步地图
            if (asciiRenderer != null)
            {
                // 确保ASCIIRenderer已初始化
                Invoke("SyncMapFromASCIIRenderer", 0.5f);
            }
            
            // 订阅区块变化事件
            GameController.Instance.PlayerController.OnPlayerChangedChunk += OnPlayerChangedChunk;
            
            // 确保PlayerController正确连接
            try 
            {
                // 手动添加Update函数来检查玩家位置变化
                InvokeRepeating("CheckPlayerMovement", 0.5f, 0.1f);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"ASCIITilemapRenderer: 连接PlayerController时出错: {e.Message}");
            }
        }
        
        private Vector2Int lastPlayerPosition;
        
        private void CheckPlayerMovement()
        {
            if (GameController.Instance == null || GameController.Instance.PlayerController == null)
                return;
                
            var currentPos = GameController.Instance.PlayerController.PlayerPosition;
            if (currentPos != lastPlayerPosition)
            {
                lastPlayerPosition = currentPos;
                if (debugMode) Debug.Log($"ASCIITilemapRenderer: 检测到玩家移动到 {currentPos}");
                UpdatePlayerPosition();
            }
        }
        
        private void OnDisable()
        {
            // 取消订阅事件
            if (GameController.Instance != null && GameController.Instance.PlayerController != null)
            {
                GameController.Instance.PlayerController.OnPlayerChangedChunk -= OnPlayerChangedChunk;
            }
            
            CancelInvoke("CheckPlayerMovement");
        }
        
        private void InitializeUI()
        {
            // 从GameController获取UIDocument
            var uiController = GameController.Instance?.UIController;
            if (uiController == null || uiController.uiDocument == null)
            {
                Debug.LogError("ASCIITilemapRenderer: 无法从GameController获取UIDocument");
                return;
            }
            
            root = uiController.uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("ASCIITilemapRenderer: 无法获取UI根元素");
                return;
            }
            
            gameMap = root.Q<VisualElement>("game-map");
            if (gameMap == null)
            {
                Debug.LogError("ASCIITilemapRenderer: 无法找到game-map元素");
                return;
            }
            
            if (debugMode) Debug.Log("ASCIITilemapRenderer: UI初始化完成");
        }
        
        // 当玩家切换区块时更新
        private void OnPlayerChangedChunk(int newX, int newY)
        {
            if (debugMode) Debug.Log($"ASCIITilemapRenderer: 玩家切换区块到 {newX},{newY}");
            
            // 等待ASCIIRenderer先完成渲染
            if (asciiRenderer != null)
            {
                // 稍微延迟一下，确保ASCIIRenderer已经渲染完成
                Invoke("SyncMapFromASCIIRenderer", 0.1f);
            }
        }
        
        #region 公共方法
        
        /// <summary>
        /// 同步ASCIIRenderer已渲染的地图数据到Tilemap
        /// </summary>
        public void SyncMapFromASCIIRenderer()
        {
            if (asciiRenderer == null)
            {
                Debug.LogError("SyncMapFromASCIIRenderer: ASCIIRenderer引用为空");
                return;
            }
            
            if (debugMode) Debug.Log("ASCIITilemapRenderer: 开始从ASCIIRenderer同步地图数据");
            
            // 确保Tilemap组件已准备好
            if (!ValidateTilemaps())
            {
                Debug.LogError("SyncMapFromASCIIRenderer: Tilemap验证失败");
                return;
            }
            
            // 清除所有图块
            ClearAllTilemaps();
            
            // 获取ASCIIRenderer的mapElements
            Label[,] rendererMapElements = GetASCIIRendererMapElements();
            if (rendererMapElements == null)
            {
                Debug.LogError("SyncMapFromASCIIRenderer: 无法获取ASCIIRenderer的mapElements");
                return;
            }
            
            // 获取地形尺寸
            int mapHeight = rendererMapElements.GetLength(0);
            int mapWidth = rendererMapElements.GetLength(1);
            
            if (debugMode) Debug.Log($"SyncMapFromASCIIRenderer: 获取到地图数据，大小: {mapWidth}x{mapHeight}");
            
            // 绘制基于ASCIIRenderer数据的图块
            RenderTilesFromASCIIRendererData(rendererMapElements, mapWidth, mapHeight);
            
            if (debugMode) Debug.Log("SyncMapFromASCIIRenderer: 地图同步完成");
            
            // 触发渲染完成事件
            OnMapRendered?.Invoke();
        }
        
        /// <summary>
        /// 获取ASCIIRenderer的mapElements
        /// </summary>
        private Label[,] GetASCIIRendererMapElements()
        {
            // 使用反射获取ASCIIRenderer的mapElements字段
            var fieldInfo = typeof(ASCIIRenderer).GetField("mapElements", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
                
            if (fieldInfo == null)
            {
                Debug.LogError("GetASCIIRendererMapElements: 无法找到ASCIIRenderer的mapElements字段");
                return null;
            }
            
            return fieldInfo.GetValue(asciiRenderer) as Label[,];
        }
        
        /// <summary>
        /// 根据ASCIIRenderer的数据渲染Tilemap
        /// </summary>
        private void RenderTilesFromASCIIRendererData(Label[,] rendererMapElements, int mapWidth, int mapHeight)
        {
            if (rendererMapElements == null) return;
            
            int tilesPlaced = 0;
            
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    // 获取单元格
                    Label cell = rendererMapElements[y, x];
                    if (cell == null || string.IsNullOrEmpty(cell.text)) continue;
                    
                    // 获取字符和颜色信息
                    char tileChar = cell.text[0];
                    
                    // 根据字符确定绘制到哪个Tilemap
                    TileBase tile = null;
                    
                    // 检查玩家字符 '@'
                    if (tileChar == '@')
                    {
                        // 玩家字符处理
                        if (characterTiles.TryGetValue(tileChar, out tile))
                        {
                            // 设置到角色层
                            // 注意Y轴坐标需要反转
                            Vector3Int cellPos = new Vector3Int(x, mapHeight - 1 - y, 0);
                            characterTilemap.SetTile(cellPos, tile);
                            tilesPlaced++;
                        }
                    }
                    else
                    {
                        // 处理其他字符
                        // 先尝试地形字典
                        if (terrainTiles.TryGetValue(tileChar, out tile))
                        {
                            // 设置到地形层
                            Vector3Int cellPos = new Vector3Int(x, mapHeight - 1 - y, 0);
                            terrainTilemap.SetTile(cellPos, tile);
                            tilesPlaced++;
                        }
                        // 再尝试物体字典
                        else if (objectTiles.TryGetValue(tileChar, out tile))
                        {
                            // 设置到物体层
                            Vector3Int cellPos = new Vector3Int(x, mapHeight - 1 - y, 0);
                            objectsTilemap.SetTile(cellPos, tile);
                            tilesPlaced++;
                        }
                    }
                }
            }
            
            if (debugMode) Debug.Log($"RenderTilesFromASCIIRendererData: 放置了 {tilesPlaced} 个Tile");
        }
        
        /// <summary>
        /// 渲染UI Elements版本的游戏地图
        /// </summary>
        public void RenderUIMap()
        {
            // 我们不再需要这个方法，因为UI地图由ASCIIRenderer负责渲染
            Debug.Log("ASCIITilemapRenderer: RenderUIMap方法已弃用，UI地图由ASCIIRenderer负责渲染");
        }
        
        /// <summary>
        /// 只更新玩家位置
        /// </summary>
        public void UpdatePlayerPosition()
        {
            // 更新Tilemap中的玩家位置
            UpdateTilemapPlayerPosition();
        }
        
        /// <summary>
        /// 更新Tilemap中的玩家位置
        /// </summary>
        private void UpdateTilemapPlayerPosition()
        {
            if (characterTilemap == null) {
                Debug.LogError("UpdateTilemapPlayerPosition: characterTilemap为null");
                return;
            }
            
            // 检查是否有ASCIIRenderer
            if (asciiRenderer != null)
            {
                // 使ASCIIRenderer先更新玩家位置
                asciiRenderer.UpdatePlayerPositionOnMap();
                
                // 然后同步地图数据
                SyncMapFromASCIIRenderer();
                return;
            }
            
            // 如果没有ASCIIRenderer，回退到原有逻辑
            // 清除角色图层
            characterTilemap.ClearAllTiles();
            
            // 获取玩家位置
            var playerController = GameController.Instance.PlayerController;
            if (playerController == null) {
                Debug.LogError("UpdateTilemapPlayerPosition: playerController为null");
                return;
            }
            
            Vector2Int playerPos = playerController.PlayerPosition;
            
            // 获取地形尺寸用于边界检查
            TerrainType[,] terrain = GameController.Instance.GetCurrentTerrain();
            if (terrain == null) {
                Debug.LogError("UpdateTilemapPlayerPosition: 无法获取地形");
                return;
            }
            
            int terrainSizeY = terrain.GetLength(0);
            int terrainSizeX = terrain.GetLength(1);
            
            // 边界检查
            if (playerPos.x >= 0 && playerPos.x < terrainSizeX && 
                playerPos.y >= 0 && playerPos.y < terrainSizeY)
            {
                // 设置玩家Tile
                TileBase playerTile = GetPlayerTile();
                if (playerTile == null) {
                    Debug.LogError("UpdateTilemapPlayerPosition: 获取玩家Tile失败");
                    return;
                }
                
                // 反转Y轴坐标
                int invertedY = terrainSizeY - 1 - playerPos.y;
                
                if (debugMode) Debug.Log($"设置玩家Tile在位置: ({playerPos.x}, {invertedY})");
                characterTilemap.SetTile(new Vector3Int(playerPos.x, invertedY, 0), playerTile);
            }
            else
            {
                Debug.LogWarning($"UpdateTilemapPlayerPosition: 玩家位置超出边界: {playerPos.x}, {playerPos.y}，地形尺寸: {terrainSizeX}x{terrainSizeY}");
            }
        }
        
        /// <summary>
        /// 更新UI Elements地图中的玩家位置
        /// </summary>
        private void UpdateUIMapPlayerPosition()
        {
            // 我们不再需要这个方法，因为UI地图由ASCIIRenderer负责渲染
            Debug.Log("ASCIITilemapRenderer: UpdateUIMapPlayerPosition方法已弃用，请使用ASCIIRenderer.UpdatePlayerPositionOnMap");
        }
        
        /// <summary>
        /// 强制重新加载所有Tile
        /// </summary>
        public void ReloadTiles()
        {
            InitializeTileDictionaries();
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 创建Tilemap层级结构
        /// </summary>
        private void CreateTilemapHierarchy()
        {
            if (debugMode) Debug.Log("ASCIITilemapRenderer: 创建Tilemap层级结构");
            
            // 如果已经有Grid，不需要创建
            if (grid != null) {
                Debug.Log("ASCIITilemapRenderer: 使用已有Grid");
                return;
            }
            
            // 创建Grid
            grid = gameObject.AddComponent<Grid>();
            grid.cellSize = new Vector3(tileSize, tileSize, 0);
            Debug.Log($"ASCIITilemapRenderer: 创建Grid，单元格大小: {tileSize}x{tileSize}");
            
            // 创建地形Tilemap
            GameObject terrainObj = new GameObject("TerrainLayer");
            terrainObj.transform.SetParent(transform, false);
            terrainTilemap = terrainObj.AddComponent<Tilemap>();
            TilemapRenderer terrainRenderer = terrainObj.AddComponent<TilemapRenderer>();
            terrainRenderer.sortingOrder = 0;
            
            // 创建物体Tilemap
            GameObject objectsObj = new GameObject("ObjectsLayer");
            objectsObj.transform.SetParent(transform, false);
            objectsTilemap = objectsObj.AddComponent<Tilemap>();
            TilemapRenderer objectsRenderer = objectsObj.AddComponent<TilemapRenderer>();
            objectsRenderer.sortingOrder = 1;
            
            // 创建角色Tilemap
            GameObject charactersObj = new GameObject("CharacterLayer");
            charactersObj.transform.SetParent(transform, false);
            characterTilemap = charactersObj.AddComponent<Tilemap>();
            TilemapRenderer characterRenderer = charactersObj.AddComponent<TilemapRenderer>();
            characterRenderer.sortingOrder = 2;
            
            // 输出各个层级的信息
            Debug.Log($"ASCIITilemapRenderer: 已创建Tilemap层级结构," +
                      $"\n地形层: {(terrainTilemap != null ? "成功" : "失败")}," +
                      $"\n物体层: {(objectsTilemap != null ? "成功" : "失败")}," +
                      $"\n角色层: {(characterTilemap != null ? "成功" : "失败")}");
        }
        
        /// <summary>
        /// 初始化Tile字典
        /// </summary>
        private void InitializeTileDictionaries()
        {
            if (debugMode) Debug.Log("ASCIITilemapRenderer: 初始化Tile字典");
            
            // 清空现有字典
            terrainTiles.Clear();
            objectTiles.Clear();
            characterTiles.Clear();
            
            // 检查配置是否有效
            if (asciiConfig == null)
            {
                Debug.LogError("ASCIITilemapRenderer: ASCIIConfig为null，无法初始化Tile字典");
                return;
            }
            
            // 加载地形Tiles
            foreach (var terrain in asciiConfig.terrains)
            {
                if (terrain.sprite != null)
                {
                    terrainTiles[terrain.asciiChar] = CreateTileFromSprite(terrain.sprite);
                }
                else
                {
                    // 为没有精灵的地形创建默认颜色Tile
                    Color terrainColor = Color.green;
                    if (terrain.asciiChar == '~') terrainColor = new Color(0, 0.7f, 1f); // 水
                    else if (terrain.asciiChar == '^') terrainColor = new Color(0.7f, 0.7f, 0.7f); // 山
                    
                    terrainTiles[terrain.asciiChar] = CreateDefaultTile(terrainColor);
                }
            }
            
            // 加载城市和建筑Tiles
            foreach (var city in asciiConfig.cities)
            {
                if (city.sprite != null)
                {
                    objectTiles[city.asciiChar] = CreateTileFromSprite(city.sprite);
                }
                else
                {
                    objectTiles[city.asciiChar] = CreateDefaultTile(Color.white);
                }
            }
            
            foreach (var building in asciiConfig.buildings)
            {
                if (building.sprite != null)
                {
                    objectTiles[building.asciiChar] = CreateTileFromSprite(building.sprite);
                }
                else
                {
                    objectTiles[building.asciiChar] = CreateDefaultTile(new Color(0.8f, 0.8f, 0.2f));
                }
            }
            
            // 加载物品Tiles
            foreach (var item in asciiConfig.items)
            {
                if (item.sprite != null)
                {
                    objectTiles[item.asciiChar] = CreateTileFromSprite(item.sprite);
                }
                else
                {
                    objectTiles[item.asciiChar] = CreateDefaultTile(new Color(1f, 0.5f, 0f));
                }
            }
            
            // 加载角色Tiles
            foreach (var character in asciiConfig.characters)
            {
                if (character.sprite != null)
                {
                    characterTiles[character.asciiChar] = CreateTileFromSprite(character.sprite);
                }
                else
                {
                    characterTiles[character.asciiChar] = CreateDefaultTile(Color.yellow);
                }
            }
            
            // 确保有玩家Tile
            if (!characterTiles.ContainsKey('@') && !characterTiles.ContainsKey('P'))
            {
                // 创建默认玩家Tile - 使用更明显的黄色
                TileBase playerTile = CreateDefaultTile(Color.yellow);
                characterTiles['@'] = playerTile;
                characterTiles['P'] = playerTile;
                
                if (debugMode) Debug.Log("ASCIITilemapRenderer: 创建了默认玩家Tile");
            }
            
            if (debugMode) Debug.Log($"ASCIITilemapRenderer: 已加载 {terrainTiles.Count} 个地形Tile, " +
                  $"{objectTiles.Count} 个物体Tile, {characterTiles.Count} 个角色Tile");
        }
        
        /// <summary>
        /// 验证Tilemap组件是否已经准备好
        /// </summary>
        private bool ValidateTilemaps()
        {
            bool isValid = true;
            
            if (terrainTilemap == null)
            {
                Debug.LogError("ASCIITilemapRenderer: terrainTilemap为null");
                isValid = false;
            }
            
            if (objectsTilemap == null)
            {
                Debug.LogError("ASCIITilemapRenderer: objectsTilemap为null");
                isValid = false;
            }
            
            if (characterTilemap == null)
            {
                Debug.LogError("ASCIITilemapRenderer: characterTilemap为null");
                isValid = false;
            }
            
            return isValid;
        }
        
        /// <summary>
        /// 清除所有Tilemap
        /// </summary>
        private void ClearAllTilemaps()
        {
            if (terrainTilemap != null) terrainTilemap.ClearAllTiles();
            if (objectsTilemap != null) objectsTilemap.ClearAllTiles();
            if (characterTilemap != null) characterTilemap.ClearAllTiles();
        }
        
        /// <summary>
        /// 获取玩家Tile
        /// </summary>
        private TileBase GetPlayerTile()
        {
            // 先尝试从字典中获取
            if (characterTiles.TryGetValue('@', out TileBase playerTile))
            {
                return playerTile;
            }
            else if (characterTiles.TryGetValue('P', out playerTile))
            {
                return playerTile;
            }
            
            // 如果没有找到，创建默认玩家Tile - 使用明显的颜色
            playerTile = CreateDefaultTile(Color.yellow);
            characterTiles['@'] = playerTile;
            
            if (debugMode) Debug.LogWarning("ASCIITilemapRenderer: 创建了应急玩家Tile");
            return playerTile;
        }
        
        /// <summary>
        /// 从Sprite创建Tile
        /// </summary>
        private TileBase CreateTileFromSprite(Sprite sprite)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            return tile;
        }
        
        /// <summary>
        /// 创建默认颜色Tile
        /// </summary>
        private TileBase CreateDefaultTile(Color color)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            
            // 创建一个带边框的方块以增强可见性
            const int tileSize = 32; // 使用固定大小
            Texture2D texture = new Texture2D(tileSize, tileSize);
            Color[] pixels = new Color[tileSize * tileSize];
            
            for (int y = 0; y < tileSize; y++)
            {
                for (int x = 0; x < tileSize; x++)
                {
                    // 边框为白色
                    if (x == 0 || x == tileSize - 1 || y == 0 || y == tileSize - 1)
                        pixels[y * tileSize + x] = Color.white;
                    else
                        pixels[y * tileSize + x] = color;
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            // 创建精灵
            Sprite sprite = Sprite.Create(
                texture, 
                new Rect(0, 0, texture.width, texture.height), 
                new Vector2(0.5f, 0.5f), 
                tileSize);
                
            tile.sprite = sprite;
            return tile;
        }
        
        /// <summary>
        /// 获取地形对应字符
        /// </summary>
        private string GetTerrainChar(TerrainType terrain)
        {
            switch (terrain)
            {
                case TerrainType.Grass:
                    return "*";
                case TerrainType.Mountain:
                    return "^";
                case TerrainType.Water:
                    return "~";
                case TerrainType.Tree:
                    return "t";  // 小树
                case TerrainType.LargeTree:
                    return "T";  // 大树
                case TerrainType.Road:
                    return ".";
                case TerrainType.City:
                    return "#";
                default:
                    return " ";
            }
        }
        
        #endregion
    }
} 
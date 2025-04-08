using UnityEngine;
using System.Collections.Generic;

namespace ALUNGAMES
{
    public class GameController : MonoBehaviour
    {
        // 单例实例
        private static GameController _instance;
        
        // 公共静态访问器
        public static GameController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameController>();
                    if (_instance == null)
                    {
                        Debug.LogError("场景中没有GameController实例！");
                    }
                }
                return _instance;
            }
        }

        [SerializeField] private DeathStrandingConfig gameConfig;
        [SerializeField] private BuildingManager buildingManager;
        [SerializeField] private CityManager cityManager;
        [SerializeField] private TerrainGenerator terrainGenerator;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private TaskManager taskManager;
        [SerializeField] private UIController uiController;
        [SerializeField] private InputManager inputManager;

        // 添加UI组件引用
        [SerializeField] private ASCIIRenderer asciiRenderer;
        [SerializeField] private PlayerHUD playerHUD;
        [SerializeField] private TaskPanel taskPanel;
        [SerializeField] private UIInputHandler uiInputHandler;
        [SerializeField] private MonoBehaviour asciiTilemapRenderer;

        private TerrainType[,] currentTerrain;

        // 区块缓存系统
        private Dictionary<string, TerrainType[,]> chunkCache = new Dictionary<string, TerrainType[,]>();
        private Queue<string> chunkLoadOrder = new Queue<string>();
        private const int MAX_CACHED_CHUNKS = 100;

        // 提供对各组件的公共访问
        public DeathStrandingConfig GameConfig => gameConfig;
        public BuildingManager BuildingManager => buildingManager;
        public CityManager CityManager => cityManager;
        public TerrainGenerator TerrainGenerator => terrainGenerator;
        public PlayerController PlayerController => playerController;
        public TaskManager TaskManager => taskManager;
        public UIController UIController => uiController;
        public InputManager InputManager => inputManager;

        // 添加UI组件的公共访问器
        public ASCIIRenderer ASCIIRenderer => asciiRenderer;
        public PlayerHUD PlayerHUD => playerHUD;
        public TaskPanel TaskPanel => taskPanel;
        public UIInputHandler UIInputHandler => uiInputHandler;
        public MonoBehaviour ASCIITilemapRenderer => asciiTilemapRenderer;

        public int CurrentWorldX => playerController.GetCurrentWorldX();
        public int CurrentWorldY => playerController.GetCurrentWorldY();

        private void Awake()
        {
            // 单例设置
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            
            // 验证组件
            if (!ValidateComponents())
            {
                Debug.LogError("GameController: 组件验证失败，游戏可能无法正常运行");
                return;
            }
        }

        private void Start()
        {
            // 初始化游戏
            InitializeGame();
        }

        // 修改ValidateComponents方法以验证UI组件
        private bool ValidateComponents()
        {
            bool valid = true;

            if (gameConfig == null)
            {
                Debug.LogError("GameController: gameConfig为null");
                valid = false;
            }

            if (buildingManager == null)
            {
                Debug.LogError("GameController: buildingManager为null");
                valid = false;
            }

            if (cityManager == null)
            {
                Debug.LogError("GameController: cityManager为null");
                valid = false;
            }

            if (terrainGenerator == null)
            {
                Debug.LogError("GameController: terrainGenerator为null");
                valid = false;
            }

            if (playerController == null)
            {
                Debug.LogError("GameController: playerController为null");
                valid = false;
            }

            if (taskManager == null)
            {
                Debug.LogError("GameController: taskManager为null");
                valid = false;
            }

            if (uiController == null)
            {
                Debug.LogError("GameController: uiController为null");
                valid = false;
            }

            // 验证UI组件
            if (asciiRenderer == null)
            {
                Debug.LogWarning("GameController: asciiRenderer为null");
                // 尝试从UIController获取
                if (uiController != null)
                    asciiRenderer = uiController.GetComponent<ASCIIRenderer>();
            }

            if (playerHUD == null)
            {
                Debug.LogWarning("GameController: playerHUD为null");
                // 尝试从UIController获取
                if (uiController != null)
                    playerHUD = uiController.GetComponent<PlayerHUD>();
            }

            if (taskPanel == null)
            {
                Debug.LogWarning("GameController: taskPanel为null");
                // 尝试从UIController获取
                if (uiController != null)
                    taskPanel = uiController.GetComponent<TaskPanel>();
            }

            if (uiInputHandler == null)
            {
                Debug.LogWarning("GameController: uiInputHandler为null");
                // 尝试从UIController获取
                if (uiController != null)
                    uiInputHandler = uiController.GetComponent<UIInputHandler>();
            }
            
            if (asciiTilemapRenderer == null)
            {
                Debug.LogWarning("GameController: asciiTilemapRenderer为null");
                // 尝试从场景中查找
                GameObject tilemapRendererGO = GameObject.Find("ASCIITilemapRenderer");
                if (tilemapRendererGO != null)
                {
                    var tilemapRendererType = System.Type.GetType("ALUNGAMES.ASCIITilemapRenderer, Assembly-CSharp");
                    if (tilemapRendererType != null)
                    {
                        asciiTilemapRenderer = tilemapRendererGO.GetComponent(tilemapRendererType) as MonoBehaviour;
                    }
                }
            }

            // InputManager是可选的，不要将其设为必要组件
            if (inputManager == null)
            {
                Debug.LogWarning("GameController: inputManager为null，将无法使用键盘/手柄控制");
            }

            return valid;
        }

        // 初始化游戏
        private void InitializeGame()
        {
            Debug.Log("正在初始化游戏...");

            // 确保gameConfig不为null
            if (gameConfig == null)
            {
                gameConfig = ScriptableObject.CreateInstance<DeathStrandingConfig>();
                Debug.LogWarning("创建了默认的游戏配置，因为原配置为null");
            }

            // 先初始化城市
            if (cityManager != null)
            {
                cityManager.InitCities(gameConfig.cityCount, gameConfig.worldWidth, gameConfig.worldHeight);
                Debug.Log($"已初始化 {gameConfig.cityCount} 个城市");
            }
            else
            {
                Debug.LogError("CityManager为null，无法初始化城市");
                return;
            }

            // 生成初始地形
            GenerateTerrain();

            // 确保地形生成成功
            if (currentTerrain == null)
            {
                Debug.LogError("地形生成失败，创建默认地形");
                CreateDefaultTerrain();
            }

            // 初始化玩家
            if (playerController != null)
            {
                playerController.InitializePlayer(currentTerrain);
                // 订阅玩家事件
                playerController.OnPlayerChangedChunk += OnPlayerChangedChunk;
            }
            else
            {
                Debug.LogError("PlayerController为null，无法初始化玩家");
                return;
            }

            // 更新UI
            if (uiController != null)
            {
                uiController.UpdateAllUI();
            }
            else
            {
                Debug.LogWarning("UIController为null，无法更新UI");
            }
            
            //强制渲染地图
            if (playerController != null)
            {
                playerController.ForceMapRender();
            }

            Debug.Log("游戏初始化完成");
        }

        // 添加默认地形创建方法
        private void CreateDefaultTerrain()
        {
            int defaultWidth = gameConfig != null ? gameConfig.mapWidth : 40;
            int defaultHeight = gameConfig != null ? gameConfig.mapHeight : 30;
            currentTerrain = new TerrainType[defaultHeight, defaultWidth];

            // 填充默认地形
            for (int y = 0; y < defaultHeight; y++)
            {
                for (int x = 0; x < defaultWidth; x++)
                {
                    currentTerrain[y, x] = TerrainType.Road;
                }
            }

            Debug.Log($"创建了默认地形 {defaultHeight}x{defaultWidth}");
        }

        // 获取区块缓存键
        private string GetChunkKey(int worldX, int worldY)
        {
            return $"{worldX},{worldY}";
        }

        // 从缓存中获取区块
        private TerrainType[,] GetChunkFromCache(int worldX, int worldY)
        {
            string key = GetChunkKey(worldX, worldY);
            if (chunkCache.TryGetValue(key, out TerrainType[,] terrain))
            {
                Debug.Log($"从缓存加载区块: {key}");
                return terrain;
            }
            return null;
        }

        // 将区块保存到缓存
        private void SaveChunkToCache(int worldX, int worldY, TerrainType[,] terrain)
        {
            string key = GetChunkKey(worldX, worldY);
            
            // 如果这个区块已经在缓存中，先移除旧的加载顺序记录
            if (chunkCache.ContainsKey(key))
            {
                var tempQueue = new Queue<string>();
                while (chunkLoadOrder.Count > 0)
                {
                    string k = chunkLoadOrder.Dequeue();
                    if (k != key) tempQueue.Enqueue(k);
                }
                chunkLoadOrder = tempQueue;
            }
            
            // 添加新的区块到缓存
            chunkCache[key] = terrain;
            chunkLoadOrder.Enqueue(key);
            
            // 如果缓存超过最大限制，移除最早的区块
            while (chunkCache.Count > MAX_CACHED_CHUNKS && chunkLoadOrder.Count > 0)
            {
                string oldestKey = chunkLoadOrder.Dequeue();
                chunkCache.Remove(oldestKey);
                Debug.Log($"移除最早的区块缓存: {oldestKey}");
            }
            
            Debug.Log($"保存区块到缓存: {key}");
        }

        // 修改生成地形的方法
        private void GenerateTerrain()
        {
            try
            {
                Debug.Log("正在生成/加载地形...");

                if (terrainGenerator == null)
                {
                    Debug.LogError("TerrainGenerator为null，无法生成地形");
                    CreateDefaultTerrain();
                    return;
                }

                int currentX = playerController.GetCurrentWorldX();
                int currentY = playerController.GetCurrentWorldY();
                
                // 先尝试从缓存加载
                TerrainType[,] cachedTerrain = GetChunkFromCache(currentX, currentY);
                if (cachedTerrain != null)
                {
                    currentTerrain = cachedTerrain;
                    Debug.Log($"从缓存加载区块 ({currentX},{currentY})");
                }
                else
                {
                    // 如果缓存中没有，生成新的地形
                    int mapWidth = gameConfig != null ? gameConfig.mapWidth : 40;
                    int mapHeight = gameConfig != null ? gameConfig.mapHeight : 30;

                    currentTerrain = terrainGenerator.GenerateTerrain(
                        mapWidth,
                        mapHeight,
                        currentX,
                        currentY
                    );

                    // 保存到缓存
                    if (currentTerrain != null)
                    {
                        SaveChunkToCache(currentX, currentY, currentTerrain);
                        Debug.Log($"生成并缓存新区块 ({currentX},{currentY})");
                    }
                }

                // 验证地形
                if (currentTerrain == null)
                {
                    Debug.LogError("地形生成/加载失败，创建默认地形");
                    CreateDefaultTerrain();
                }
                else
                {
                    Debug.Log($"当前地形大小: {currentTerrain.GetLength(0)}x{currentTerrain.GetLength(1)}");
                }

                // 更新玩家的地形引用
                playerController.SetTerrain(currentTerrain);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成/加载地形时出错: {e.Message}");
                CreateDefaultTerrain();
            }
        }

        // 当玩家切换世界区块
        private void OnPlayerChangedChunk(int newX, int newY)
        {
            // 重新生成地形
            GenerateTerrain();

            // 更新UI - 使用专门的方法更新世界地图和游戏地图
            uiController.UpdateWorldAndMaps(newX, newY);
        }

        // 获取当前地形
        public TerrainType[,] GetCurrentTerrain()
        {
            // 如果地形为null，创建一个默认的
            if (currentTerrain == null)
            {
                Debug.LogWarning("GetCurrentTerrain: 请求地形时发现地形为null，创建默认地形");
                CreateDefaultTerrain();
            }

            return currentTerrain;
        }
    }
}
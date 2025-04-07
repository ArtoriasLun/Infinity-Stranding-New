using UnityEngine;

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
            // 实现单例模式
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("场景中存在多个GameController实例。销毁此实例。");
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // 验证所有必要的组件是否存在
            if (!ValidateComponents())
            {
                Debug.LogError("GameController缺少必要组件，请检查Inspector面板中的引用");
                return;
            }

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

            // 初始化城市   
            cityManager.InitCities(gameConfig.cityCount, gameConfig.worldWidth, gameConfig.worldHeight);

            // 生成初始地形
            GenerateTerrain();

            // 确保地形生成成功
            if (currentTerrain == null)
            {
                Debug.LogError("地形生成失败，创建默认地形");
                CreateDefaultTerrain();
            }

            // 初始化玩家
            playerController.InitializePlayer(currentTerrain);

            // 订阅玩家事件
            playerController.OnPlayerChangedChunk += OnPlayerChangedChunk;

            // 更新UI
            uiController.UpdateAllUI();
            
            //强制渲染地图
            playerController.ForceMapRender();

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

        // 生成地形
        private void GenerateTerrain()
        {
            try
            {
                Debug.Log("正在生成地形...");

                // 确保terrainGenerator不为null
                if (terrainGenerator == null)
                {
                    Debug.LogError("TerrainGenerator为null，无法生成地形");
                    CreateDefaultTerrain();
                    return;
                }

                // 从gameConfig获取地图大小，如果为null则使用默认值
                int mapWidth = gameConfig != null ? gameConfig.mapWidth : 40;
                int mapHeight = gameConfig != null ? gameConfig.mapHeight : 30;

                currentTerrain = terrainGenerator.GenerateTerrain(
                    mapWidth,
                    mapHeight,
                    playerController.GetCurrentWorldX(),
                    playerController.GetCurrentWorldY()
                );

                // 验证生成的地形
                if (currentTerrain == null)
                {
                    Debug.LogError("TerrainGenerator返回了null地形，创建默认地形");
                    CreateDefaultTerrain();
                }
                else
                {
                    Debug.Log($"成功生成地形: {currentTerrain.GetLength(0)}x{currentTerrain.GetLength(1)}");
                }

                // 更新玩家的地形引用
                playerController.SetTerrain(currentTerrain);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成地形时出错: {e.Message}");
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
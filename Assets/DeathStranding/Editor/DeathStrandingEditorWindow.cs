using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace ALUNGAMES
{
    /// <summary>
    /// 死亡搁浅游戏编辑器窗口
    /// 用于配置游戏参数和在场景中设置游戏对象
    /// </summary>
    public class DeathStrandingEditorWindow : EditorWindow
    {
        private DeathStrandingConfig gameConfig;
        private Dictionary<string, VisualElement> configFields = new Dictionary<string, VisualElement>();

        //===================================================
        // 窗口初始化
        //===================================================

        [MenuItem("😊/Game Editor")]
        public static void ShowWindow()
        {
            DeathStrandingEditorWindow wnd = GetWindow<DeathStrandingEditorWindow>();
            wnd.titleContent = new GUIContent("Death Stranding Editor");
            
            // 设置窗口最小尺寸，确保有足够空间显示滚动视图
            wnd.minSize = new Vector2(450, 600);
        }

        public void CreateGUI()
        {
            // 加载或创建配置
            LoadOrCreateConfig();

            // 创建根容器
            var root = rootVisualElement;

            // 添加样式
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/DeathStranding/Editor/DeathStrandingEditor.uss");
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }

            //---------------------------------------------------
            // 创建主界面元素
            //---------------------------------------------------

            // 创建主标题
            var titleLabel = new Label("Death Stranding Game Editor");
            titleLabel.AddToClassList("title-label");
            root.Add(titleLabel);

            // 创建主要按钮
            var setupButton = new Button(SetupGame) { text = "在场景中设置游戏" };
            setupButton.AddToClassList("setup-button");
            root.Add(setupButton);

            // 添加分割线
            var separator = new VisualElement();
            separator.AddToClassList("separator");
            root.Add(separator);

            //---------------------------------------------------
            // 创建配置部分
            //---------------------------------------------------

            // 创建滚动视图
            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1; // 允许滚动视图填满剩余空间
            root.Add(scrollView);

            // 在滚动视图中创建配置部分
            var configSection = new VisualElement();
            configSection.AddToClassList("config-section");
            scrollView.Add(configSection);

            // 添加标题
            var configTitle = new Label("游戏配置");
            configTitle.AddToClassList("section-title");
            configSection.Add(configTitle);

            // 玩家配置
            AddConfigCategory(configSection, "玩家属性");
            AddIntField(configSection, "maxCargo", "最大携带货物数量", gameConfig.maxCargo);
            AddIntField(configSection, "maxStrain", "最大疲劳值", gameConfig.maxStrain);
            AddIntField(configSection, "moveSpeed", "基础移动速度", gameConfig.moveSpeed);

            // 地形配置
            AddConfigCategory(configSection, "地形影响");

            // 河流配置
            AddFloatField(configSection, "riverStrainChance", "河流增加疲劳几率", gameConfig.terrain.river.strainChance, true);
            AddIntField(configSection, "riverStrainAmount", "河流增加疲劳量", gameConfig.terrain.river.strainAmount, true);
            AddIntField(configSection, "riverMoveResistance", "河流移动阻力", gameConfig.terrain.river.moveResistance, true);

            // 山脉配置
            AddFloatField(configSection, "mountainStrainChance", "山脉增加疲劳几率", gameConfig.terrain.mountain.strainChance, true);
            AddIntField(configSection, "mountainStrainAmount", "山脉增加疲劳量", gameConfig.terrain.mountain.strainAmount, true);
            AddIntField(configSection, "mountainMoveResistance", "山脉移动阻力", gameConfig.terrain.mountain.moveResistance, true);

            // 世界生成配置
            AddConfigCategory(configSection, "世界生成参数");
            AddIntField(configSection, "mapWidth", "地图宽度", gameConfig.mapWidth);
            AddIntField(configSection, "mapHeight", "地图高度", gameConfig.mapHeight);
            AddIntField(configSection, "worldWidth", "世界宽度", gameConfig.worldWidth);
            AddIntField(configSection, "worldHeight", "世界高度", gameConfig.worldHeight);
            AddIntField(configSection, "minCityCount", "最小城市数量", gameConfig.minCityCount);
            AddIntField(configSection, "maxCityCount", "最大城市数量", gameConfig.maxCityCount);
            AddIntField(configSection, "cityCount", "城市数量", gameConfig.cityCount);

            // 地形生成配置
            AddFloatField(configSection, "mountainThreshold", "山脉生成阈值", gameConfig.mountainThreshold);
            AddFloatField(configSection, "grassThreshold", "草地生成阈值", gameConfig.grassThreshold);
            AddFloatField(configSection, "grassChance", "草地生成概率", gameConfig.grassChance);

            // 河流生成配置
            AddIntField(configSection, "riverCount", "每区块河流数", gameConfig.riverCount);
            AddIntField(configSection, "riverMinLength", "河流最小长度", gameConfig.riverMinLength);
            AddFloatField(configSection, "riverBranchChance", "河流分支概率", gameConfig.riverBranchChance);

            // 树木生成配置
            AddIntField(configSection, "treeMaxCount", "最大树木数量", gameConfig.treeMaxCount);

            // 任务配置
            AddConfigCategory(configSection, "任务系统");
            AddFloatField(configSection, "bitcoinReward", "每个货物比特币奖励", gameConfig.tasks.bitcoinReward);

            // 游戏机制配置
            AddConfigCategory(configSection, "游戏机制");
            AddFloatField(configSection, "strainDropChance", "疲劳掉货几率", gameConfig.strainDropChance);

            // 添加保存配置按钮
            var saveConfigButton = new Button(SaveConfig) { text = "保存配置" };
            saveConfigButton.AddToClassList("config-button");
            configSection.Add(saveConfigButton);

            // 注意：滚动视图已经在前面添加到root，这里不需要重复添加
            // root.Add(scrollView);
        }

        //===================================================
        // UI帮助方法
        //===================================================

        /// <summary>
        /// 添加配置类别标题
        /// </summary>
        private void AddConfigCategory(VisualElement parent, string categoryName)
        {
            var categoryLabel = new Label(categoryName);
            categoryLabel.AddToClassList("category-label");
            parent.Add(categoryLabel);
        }

        /// <summary>
        /// 添加整数配置字段
        /// </summary>
        private void AddIntField(VisualElement parent, string fieldName, string displayName, int value, bool indent = false)
        {
            var field = new IntegerField(displayName) { value = value };
            field.AddToClassList(indent ? "config-field-indent" : "config-field");
            parent.Add(field);
            configFields[fieldName] = field;
        }

        /// <summary>
        /// 添加浮点数配置字段
        /// </summary>
        private void AddFloatField(VisualElement parent, string fieldName, string displayName, float value, bool indent = false)
        {
            var field = new FloatField(displayName) { value = value };
            field.AddToClassList(indent ? "config-field-indent" : "config-field");
            parent.Add(field);
            configFields[fieldName] = field;
        }

        //===================================================
        // 配置管理
        //===================================================

        /// <summary>
        /// 加载或创建游戏配置
        /// </summary>
        private void LoadOrCreateConfig()
        {
            // 尝试加载现有配置
            gameConfig = AssetDatabase.LoadAssetAtPath<DeathStrandingConfig>("Assets/DeathStranding/DeathStrandingConfig.asset");

            // 如果不存在则创建新配置
            if (gameConfig == null)
            {
                gameConfig = ScriptableObject.CreateInstance<DeathStrandingConfig>();

                // 确保目录存在
                if (!Directory.Exists("Assets/DeathStranding"))
                {
                    Directory.CreateDirectory("Assets/DeathStranding");
                }

                // 创建资源文件
                AssetDatabase.CreateAsset(gameConfig, "Assets/DeathStranding/DeathStrandingConfig.asset");
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// 保存配置到ScriptableObject
        /// </summary>
        private void SaveConfig()
        {
            // 获取并保存所有配置值
            Undo.RecordObject(gameConfig, "Update Death Stranding Config");

            // 玩家配置
            gameConfig.maxCargo = ((IntegerField)configFields["maxCargo"]).value;
            gameConfig.maxStrain = ((IntegerField)configFields["maxStrain"]).value;
            gameConfig.moveSpeed = ((IntegerField)configFields["moveSpeed"]).value;

            // 河流配置
            gameConfig.terrain.river.strainChance = ((FloatField)configFields["riverStrainChance"]).value;
            gameConfig.terrain.river.strainAmount = ((IntegerField)configFields["riverStrainAmount"]).value;
            gameConfig.terrain.river.moveResistance = ((IntegerField)configFields["riverMoveResistance"]).value;

            // 山脉配置
            gameConfig.terrain.mountain.strainChance = ((FloatField)configFields["mountainStrainChance"]).value;
            gameConfig.terrain.mountain.strainAmount = ((IntegerField)configFields["mountainStrainAmount"]).value;
            gameConfig.terrain.mountain.moveResistance = ((IntegerField)configFields["mountainMoveResistance"]).value;

            // 世界生成配置
            gameConfig.mapWidth = ((IntegerField)configFields["mapWidth"]).value;
            gameConfig.mapHeight = ((IntegerField)configFields["mapHeight"]).value;
            gameConfig.worldWidth = ((IntegerField)configFields["worldWidth"]).value;
            gameConfig.worldHeight = ((IntegerField)configFields["worldHeight"]).value;
            gameConfig.minCityCount = Mathf.Clamp(((IntegerField)configFields["minCityCount"]).value, 2, 10);
            gameConfig.maxCityCount = Mathf.Clamp(((IntegerField)configFields["maxCityCount"]).value, gameConfig.minCityCount, 10);
            gameConfig.cityCount = Mathf.Clamp(((IntegerField)configFields["cityCount"]).value, gameConfig.minCityCount, gameConfig.maxCityCount);

            // 地形生成配置
            gameConfig.mountainThreshold = ((FloatField)configFields["mountainThreshold"]).value;
            gameConfig.grassThreshold = ((FloatField)configFields["grassThreshold"]).value;
            gameConfig.grassChance = ((FloatField)configFields["grassChance"]).value;

            // 河流生成配置
            gameConfig.riverCount = ((IntegerField)configFields["riverCount"]).value;
            gameConfig.riverMinLength = ((IntegerField)configFields["riverMinLength"]).value;
            gameConfig.riverBranchChance = ((FloatField)configFields["riverBranchChance"]).value;

            // 树木生成配置
            gameConfig.treeMaxCount = ((IntegerField)configFields["treeMaxCount"]).value;

            // 任务配置
            gameConfig.tasks.bitcoinReward = ((FloatField)configFields["bitcoinReward"]).value;

            // 游戏机制配置
            gameConfig.strainDropChance = ((FloatField)configFields["strainDropChance"]).value;

            // 保存配置
            EditorUtility.SetDirty(gameConfig);
            AssetDatabase.SaveAssets();

            Debug.Log("配置已保存");
        }

        //===================================================
        // 游戏设置
        //===================================================

        /// <summary>
        /// 在场景中设置游戏所需的所有组件
        /// </summary>
        private void SetupGame()
        {
            Debug.Log("开始设置游戏...");
            
            // 1. 查找或创建GameController
            GameObject controllerGO = null;
            GameController controller = GameObject.FindObjectOfType<GameController>();
            
            if (controller != null)
            {
                if (!EditorUtility.DisplayDialog("警告", "场景中已存在游戏控制器。是否要重新配置？", "是", "否"))
                {
                    return;
                }
                controllerGO = controller.gameObject;
            }
            else
            {
                controllerGO = new GameObject("DSGameController");
                controller = controllerGO.AddComponent<GameController>();
            }

            // 2. 添加或获取管理器组件
            var buildingManager = GetOrAddComponent<BuildingManager>(controllerGO);
            var cityManager = GetOrAddComponent<CityManager>(controllerGO);
            var terrainGenerator = GetOrAddComponent<TerrainGenerator>(controllerGO);
            var playerController = GetOrAddComponent<PlayerController>(controllerGO);
            var taskManager = GetOrAddComponent<TaskManager>(controllerGO);
            var inputManager = GetOrAddComponent<InputManager>(controllerGO);
            
            // 3. 查找或创建UIDocument
            GameObject uiDocumentGO = GameObject.Find("UI_DeathStranding");
            UIDocument uiDocument = null;
            
            if (uiDocumentGO == null)
            {
                uiDocumentGO = new GameObject("UI_DeathStranding");
                uiDocument = uiDocumentGO.AddComponent<UIDocument>();
            }
            else
            {
                uiDocument = uiDocumentGO.GetComponent<UIDocument>();
                if (uiDocument == null)
                {
                    uiDocument = uiDocumentGO.AddComponent<UIDocument>();
                }
            }
            
            // 4. 设置UXML
            var uxmlPath = "Assets/DeathStranding/UI/DeathStranding.uxml";
            var uxmlAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            if (uxmlAsset == null)
            {
                Debug.LogError($"找不到UXML文件: {uxmlPath}");
                return;
            }
            uiDocument.visualTreeAsset = uxmlAsset;
            
            // 5. 获取或添加UI组件
            var uiController = GetOrAddComponent<UIController>(uiDocumentGO);
            var asciiRenderer = GetOrAddComponent<ASCIIRenderer>(uiDocumentGO);
            var playerHUD = GetOrAddComponent<PlayerHUD>(uiDocumentGO);
            var taskPanel = GetOrAddComponent<TaskPanel>(uiDocumentGO);
            var uiInputHandler = GetOrAddComponent<UIInputHandler>(uiDocumentGO);
            
            // 6. 创建或获取ASCIITilemapRenderer
            GameObject tilemapRendererGO = GameObject.Find("ASCIITilemapRenderer");
            MonoBehaviour tilemapRenderer = null;
            
            if (tilemapRendererGO == null)
            {
                tilemapRendererGO = new GameObject("ASCIITilemapRenderer");
                // 使用反射添加组件
                var tilemapRendererType = System.Type.GetType("ALUNGAMES.ASCIITilemapRenderer, Assembly-CSharp");
                if (tilemapRendererType != null)
                {
                    tilemapRenderer = tilemapRendererGO.AddComponent(tilemapRendererType) as MonoBehaviour;
                }
                else
                {
                    Debug.LogError("无法找到ASCIITilemapRenderer类型");
                }
            }
            else
            {
                // 使用反射获取组件
                var tilemapRendererType = System.Type.GetType("ALUNGAMES.ASCIITilemapRenderer, Assembly-CSharp");
                if (tilemapRendererType != null)
                {
                    tilemapRenderer = tilemapRendererGO.GetComponent(tilemapRendererType) as MonoBehaviour;
                    if (tilemapRenderer == null)
                    {
                        tilemapRenderer = tilemapRendererGO.AddComponent(tilemapRendererType) as MonoBehaviour;
                    }
                }
                else
                {
                    Debug.LogError("无法找到ASCIITilemapRenderer类型");
                }
            }
            
            // 6.1 配置ASCIITilemapRenderer
            // 查找ASCIIConfig
            var asciiConfig = AssetDatabase.FindAssets("t:ASCIIConfig")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ASCIIConfig>)
                .FirstOrDefault();
                
            if (asciiConfig != null)
            {
                // 使用反射设置ASCIIConfig
                SetPrivateField(tilemapRenderer, "asciiConfig", asciiConfig);
            }
            else
            {
                Debug.LogWarning("找不到ASCIIConfig资源，ASCIITilemapRenderer可能无法正常工作");
            }
            
            // 7. 设置相机跟随组件
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                // 如果没有主相机，创建一个
                GameObject cameraGO = new GameObject("Main Camera");
                mainCamera = cameraGO.AddComponent<Camera>();
                mainCamera.tag = "MainCamera";
                
                // 设置基本相机属性
                mainCamera.orthographic = true; // 使用正交相机适合2D游戏
                mainCamera.orthographicSize = 15; // 合适的大小以显示地图
                mainCamera.backgroundColor = Color.black;
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                
                Debug.Log("创建了新的主相机");
            }
            
            // 添加或获取CameraFollow组件（通过反射）
            var cameraFollowType = System.Type.GetType("ALUNGAMES.CameraFollow, Assembly-CSharp");
            if (cameraFollowType != null)
            {
                MonoBehaviour cameraFollow = mainCamera.gameObject.GetComponent(cameraFollowType) as MonoBehaviour;
                if (cameraFollow == null)
                {
                    cameraFollow = mainCamera.gameObject.AddComponent(cameraFollowType) as MonoBehaviour;
                    
                    // 使用反射设置offset，保持相机在玩家上方
                    var setOffsetMethod = cameraFollowType.GetMethod("SetOffset");
                    if (setOffsetMethod != null)
                    {
                        setOffsetMethod.Invoke(cameraFollow, new object[] { new Vector3(0, 0, -10) });
                    }
                    
                    Debug.Log("添加了CameraFollow组件到主相机");
                }
            }
            else
            {
                Debug.LogError("无法找到CameraFollow类型，请确保该类已定义在正确的命名空间中");
            }
            
            // 8. 配置UI组件，设置UIDocument引用
            ConfigureAllUIComponents(uiDocumentGO, uiDocument);
            
            // 9. 设置GameController引用的私有字段
            // 核心管理器引用
            SetPrivateField(controller, "gameConfig", gameConfig);
            SetPrivateField(controller, "buildingManager", buildingManager);
            SetPrivateField(controller, "cityManager", cityManager);
            SetPrivateField(controller, "terrainGenerator", terrainGenerator);
            SetPrivateField(controller, "playerController", playerController);
            SetPrivateField(controller, "taskManager", taskManager);
            SetPrivateField(controller, "uiController", uiController);
            SetPrivateField(controller, "inputManager", inputManager);
            
            // UI组件引用
            SetPrivateField(controller, "asciiRenderer", asciiRenderer);
            SetPrivateField(controller, "playerHUD", playerHUD);
            SetPrivateField(controller, "taskPanel", taskPanel);
            SetPrivateField(controller, "uiInputHandler", uiInputHandler);
            
            // 添加ASCIITilemapRenderer引用
            SetPrivateField(controller, "asciiTilemapRenderer", tilemapRenderer);
            
            // 10. 选中游戏控制器
            Selection.activeGameObject = controllerGO;
            EditorUtility.SetDirty(controller);

            // 验证UI组件是否成功设置到GameController中
            bool uiComponentsValid = controller.ASCIIRenderer != null && 
                                   controller.PlayerHUD != null && 
                                   controller.TaskPanel != null && 
                                   controller.UIInputHandler != null;
            
            if (!uiComponentsValid)
            {
                Debug.LogWarning("部分UI组件未成功配置到GameController中");
            }

            Debug.Log("死亡搁浅游戏设置完成！");
        }

        /// <summary>
        /// 设置私有字段值
        /// </summary>
        private void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Public);
                
            if (field != null)
            {
                field.SetValue(target, value);
            }
            else
            {
                Debug.LogWarning($"找不到字段: {fieldName}");
            }
        }

        //===================================================
        // 辅助方法
        //===================================================

        /// <summary>
        /// 获取或添加组件
        /// </summary>
        private T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// 配置所有UI组件的UIDocument引用
        /// </summary>
        private void ConfigureAllUIComponents(GameObject uiDocumentGO, UIDocument uiDocument)
        {
            // 1. 获取所有UI组件
            var uiController = GetOrAddComponent<UIController>(uiDocumentGO);
            var asciiRenderer = GetOrAddComponent<ASCIIRenderer>(uiDocumentGO);
            var playerHUD = GetOrAddComponent<PlayerHUD>(uiDocumentGO);
            var taskPanel = GetOrAddComponent<TaskPanel>(uiDocumentGO);
            var uiInputHandler = GetOrAddComponent<UIInputHandler>(uiDocumentGO);
            
            // 2. 为所有UI组件设置UIDocument引用
            SetPrivateField(uiController, "uiDocument", uiDocument);
            SetPrivateField(asciiRenderer, "uiDocument", uiDocument);
            SetPrivateField(playerHUD, "uiDocument", uiDocument);
            SetPrivateField(taskPanel, "uiDocument", uiDocument);
            SetPrivateField(uiInputHandler, "uiDocument", uiDocument);
        }
    }
}
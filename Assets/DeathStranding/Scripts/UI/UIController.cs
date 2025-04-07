using UnityEngine;
using UnityEngine.UIElements;

namespace ALUNGAMES
{
    /// <summary>
    /// 协调所有UI组件的控制器
    /// 这个新版本将各个UI职责分离到不同组件中
    /// </summary>
    public class UIController : MonoBehaviour
    {
        [SerializeField] public UIDocument uiDocument;
        
        private void OnEnable()
        {
            InitializeComponents();
            
            var playerController = GameController.Instance.PlayerController;
            if (playerController != null)
            {
                // 订阅玩家移动和区块变化事件
                playerController.OnPlayerMoved += UpdatePlayerInfo;
                playerController.OnPlayerChangedChunk += UpdateWorldAndMaps;
            }
        }
        
        private void OnDisable()
        {
            var playerController = GameController.Instance.PlayerController;
            if (playerController != null)
            {
                // 取消订阅事件
                playerController.OnPlayerMoved -= UpdatePlayerInfo;
                playerController.OnPlayerChangedChunk -= UpdateWorldAndMaps;
            }
        }
        
        // 初始化所有UI组件
        private void InitializeComponents()
        {
            // 直接使用GameController.Instance中的UI组件引用
            var asciiRenderer = GameController.Instance.ASCIIRenderer;
            var playerHUD = GameController.Instance.PlayerHUD;
            var taskPanel = GameController.Instance.TaskPanel;
            var inputHandler = GameController.Instance.UIInputHandler;
            
            // 如果组件存在则初始化
            if (asciiRenderer != null)
                asciiRenderer.Initialize();
                
            if (playerHUD != null)
                playerHUD.Initialize();
                
            if (taskPanel != null)
                taskPanel.Initialize();
                
            if (inputHandler != null)
                inputHandler.Initialize();
        }
        
        // 完整更新UI (用于初始化或者手动调用)
        public void UpdateAllUI()
        {
            // 直接使用GameController.Instance中的UI组件引用
            var asciiRenderer = GameController.Instance.ASCIIRenderer;
            var playerHUD = GameController.Instance.PlayerHUD;
            var taskPanel = GameController.Instance.TaskPanel;
            
            if (playerHUD != null)
                playerHUD.UpdateHUD();
                
            if (taskPanel != null)
                taskPanel.UpdateTaskPanel();
                
            if (asciiRenderer != null)
            {
                asciiRenderer.RenderMap();
                asciiRenderer.RenderWorldMap();
            }
        }
        
        // 轻量级更新 - 仅在玩家移动时更新
        public void UpdatePlayerInfo()
        {
            // 直接使用GameController.Instance中的UI组件引用
            var asciiRenderer = GameController.Instance.ASCIIRenderer;
            var playerHUD = GameController.Instance.PlayerHUD;
            var taskPanel = GameController.Instance.TaskPanel;
            
            if (playerHUD != null)
                playerHUD.UpdateHUD();
                
            if (taskPanel != null)
                taskPanel.UpdateTaskPanel();
                
            if (asciiRenderer != null)
                asciiRenderer.UpdatePlayerPositionOnMap();
        }
        
        // 重量级更新 - 在区块变化时重建地图
        public void UpdateWorldAndMaps(int newX, int newY)
        {
            // 直接使用GameController.Instance中的UI组件引用
            var asciiRenderer = GameController.Instance.ASCIIRenderer;
            
            if (asciiRenderer != null)
            {
                asciiRenderer.RenderMap();
                asciiRenderer.RenderWorldMap();
            }
        }
        
        /// <summary>
        /// 切换UI显示状态（开/关）
        /// </summary>
        public void ToggleUIVisibility()
        {
            // 获取UIDocument所在的GameObject
            if (uiDocument != null)
            {
                GameObject uiGameObject = uiDocument.gameObject;
                uiGameObject.SetActive(!uiGameObject.activeSelf);
                Debug.Log($"UI显示状态已切换为: {uiGameObject.activeSelf}");
            }
            else
            {
                // 尝试查找名为"UI_DeathStranding"的游戏对象作为备用方案
                GameObject uiGameObject = GameObject.Find("UI_DeathStranding");
                
                if (uiGameObject != null)
                {
                    uiGameObject.SetActive(!uiGameObject.activeSelf);
                    Debug.Log($"UI显示状态已切换为: {uiGameObject.activeSelf}");
                }
                else
                {
                    Debug.LogWarning("找不到UI游戏对象，无法切换显示状态");
                }
            }
        }
    }
} 
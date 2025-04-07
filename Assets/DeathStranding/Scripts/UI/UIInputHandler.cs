using UnityEngine;
using UnityEngine.UIElements;

namespace ALUNGAMES
{
    public class UIInputHandler : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        
        private VisualElement root;
        
        // 方向按钮
        private Button upButton;
        private Button upRightButton;
        private Button rightButton;
        private Button downRightButton;
        private Button downButton;
        private Button downLeftButton;
        private Button leftButton;
        private Button upLeftButton;
        private Button actionButton;
        
        private void OnEnable()
        {
            Initialize();
        }
        
        // 初始化
        public void Initialize()
        {
            if (uiDocument == null) return;
            
            root = uiDocument.rootVisualElement;
            
            // 获取方向按钮
            upButton = root.Q<Button>("up-button");
            upRightButton = root.Q<Button>("up-right-button");
            rightButton = root.Q<Button>("right-button");
            downRightButton = root.Q<Button>("down-right-button");
            downButton = root.Q<Button>("down-button");
            downLeftButton = root.Q<Button>("down-left-button");
            leftButton = root.Q<Button>("left-button");
            upLeftButton = root.Q<Button>("up-left-button");
            actionButton = root.Q<Button>("action-button");
            
            // 注册按钮事件
            RegisterButtonEvents();
        }
        
        // 注册按钮事件
        private void RegisterButtonEvents()
        {
            var inputManager = GameController.Instance.InputManager;
            var playerController = GameController.Instance.PlayerController;
            
            if (inputManager != null)
            {
                upButton.clicked += inputManager.MoveUp;
                upRightButton.clicked += inputManager.MoveUpRight;
                rightButton.clicked += inputManager.MoveRight;
                downRightButton.clicked += inputManager.MoveDownRight;
                downButton.clicked += inputManager.MoveDown;
                downLeftButton.clicked += inputManager.MoveDownLeft;
                leftButton.clicked += inputManager.MoveLeft;
                upLeftButton.clicked += inputManager.MoveUpLeft;
                actionButton.clicked += inputManager.Interact;
            }
            else
            {
                // 如果InputManager不可用，仍保留直接调用PlayerController的功能作为备选
                upButton.clicked += () => playerController.MovePlayer(Direction.Up);
                upRightButton.clicked += () => playerController.MovePlayer(Direction.UpRight);
                rightButton.clicked += () => playerController.MovePlayer(Direction.Right);
                downRightButton.clicked += () => playerController.MovePlayer(Direction.DownRight);
                downButton.clicked += () => playerController.MovePlayer(Direction.Down);
                downLeftButton.clicked += () => playerController.MovePlayer(Direction.DownLeft);
                leftButton.clicked += () => playerController.MovePlayer(Direction.Left);
                upLeftButton.clicked += () => playerController.MovePlayer(Direction.UpLeft);
                actionButton.clicked += () => playerController.PerformAction();
            }
        }
    }
} 
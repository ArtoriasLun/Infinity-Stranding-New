using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace ALUNGAMES
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        private Vector2 moveInput;

        private InputAction moveAction;
        private InputAction interactAction;
        private InputAction statusAction;
        public float moveTime = 0.3f, mt;
        
        // 移动模式相关变量
        [SerializeField] private bool isHoldMode = false; // 默认为点击模式
        [SerializeField] private float clickModeDelay = 0.3f; // 点击模式的移动延迟
        [SerializeField] private float holdModeDelay = 0.15f; // 长按模式的移动延迟（更快）
        private float holdModeTimer = 0f; // 用于长按检测的计时器
        private float holdThreshold = 0.5f; // 按住多久视为长按（秒）
        private bool isMoving = false; // 是否正在移动

        private void Awake()
        {
            // Get references to the action maps
            var playerActionMap = inputActions.FindActionMap("Player");

            // Get references to the specific actions
            moveAction = playerActionMap.FindAction("Move");
            interactAction = playerActionMap.FindAction("Interact");
            statusAction = playerActionMap.FindAction("Status");

            // Register callback functions for actions
            moveAction.performed += OnMoveInput;
            moveAction.canceled += OnMoveInput;

            interactAction.performed += OnInteractInput;

            statusAction.performed += OnStatusInput;
        }

        private void OnEnable()
        {
            // 启用Input Actions
            inputActions.Enable();

            // 确保在重新启用时也绑定回调函数
            if (moveAction != null)
            {
                moveAction.performed += OnMoveInput;
                moveAction.canceled += OnMoveInput;
            }

            if (interactAction != null)
            {
                interactAction.performed += OnInteractInput;
            }

            if (statusAction != null)
            {
                statusAction.performed += OnStatusInput;
            }
        }

        private void OnDisable()
        {
            // 禁用Input Actions
            inputActions.Disable();

            // Unregister callback functions to prevent memory leaks
            if (moveAction != null)
            {
                moveAction.performed -= OnMoveInput;
                moveAction.canceled -= OnMoveInput;
            }

            if (interactAction != null)
            {
                interactAction.performed -= OnInteractInput;
            }

            if (statusAction != null)
            {
                statusAction.performed -= OnStatusInput;
            }
        }

        private void OnMoveInput(InputAction.CallbackContext context)
        {
            // 获取移动输入值
            moveInput = context.ReadValue<Vector2>();
            
            // 检测按键按下或释放
            if (context.performed && moveInput.sqrMagnitude > 0.1f)
            {
                // 按键被按下
                isMoving = true;
                holdModeTimer = 0f; // 重置长按计时器
                mt = clickModeDelay; // 设置为点击模式延迟
                
                // 在点击模式下立即执行一次移动
                if (!isHoldMode)
                {
                    ProcessMovementDirection();
                }
            }
            else if (context.canceled || moveInput.sqrMagnitude < 0.1f)
            {
                // 按键被释放
                isMoving = false;
                isHoldMode = false; // 重置回点击模式
            }
        }

        private void OnInteractInput(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                // 执行交互操作，通过GameController.Instance获取PlayerController
                GameController.Instance.PlayerController.PerformAction();
            }
        }

        private void OnStatusInput(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                GameController.Instance.UIController.ToggleUIVisibility();
            }
        }

        private void Update()
        {
            // 处理移动输入
            HandleMovementInput();
        }

        private void HandleMovementInput()
        {
            // 如果没有按下移动键，直接返回
            if (!isMoving) return;
            
            // 处理长按检测
            if (isMoving && !isHoldMode)
            {
                holdModeTimer += Time.deltaTime;
                if (holdModeTimer >= holdThreshold)
                {
                    // 转换为长按模式
                    isHoldMode = true;
                    mt = 0; // 立即允许移动
                }
            }
            
            // 移动冷却处理
            if (mt > 0) 
            { 
                mt -= Time.deltaTime; 
                return; 
            }
            
            // 处理移动
            if (moveInput.sqrMagnitude > 0.1f)
            {
                // 根据当前模式设置不同的移动冷却时间
                mt = isHoldMode ? holdModeDelay : clickModeDelay;
                ProcessMovementDirection();
            }
        }
        
        private void ProcessMovementDirection()
        {
            // 8向移动处理
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
            // 将角度转换为0-360范围
            if (angle < 0) angle += 360f;

            // 根据角度确定方向
            Direction direction = GetDirectionFromAngle(angle);
            // 通过GameController.Instance获取PlayerController
            GameController.Instance.PlayerController.MovePlayer(direction);
        }

        private Direction GetDirectionFromAngle(float angle)
        {
            // 将360度分成8个方向，每个方向占45度
            // 0/360 = 右, 45 = 右上, 90 = 上, 135 = 左上, 180 = 左, 225 = 左下, 270 = 下, 315 = 右下
            if (angle >= 337.5f || angle < 22.5f) return Direction.Right;
            if (angle >= 22.5f && angle < 67.5f) return Direction.UpRight;
            if (angle >= 67.5f && angle < 112.5f) return Direction.Up;
            if (angle >= 112.5f && angle < 157.5f) return Direction.UpLeft;
            if (angle >= 157.5f && angle < 202.5f) return Direction.Left;
            if (angle >= 202.5f && angle < 247.5f) return Direction.DownLeft;
            if (angle >= 247.5f && angle < 292.5f) return Direction.Down;
            if (angle >= 292.5f && angle < 337.5f) return Direction.DownRight;

            // 默认向右
            return Direction.Right;
        }

        // 暴露方法供UI按钮调用
        public void MoveUp() { GameController.Instance.PlayerController.MovePlayer(Direction.Up); }
        public void MoveUpRight() { GameController.Instance.PlayerController.MovePlayer(Direction.UpRight); }
        public void MoveRight() { GameController.Instance.PlayerController.MovePlayer(Direction.Right); }
        public void MoveDownRight() { GameController.Instance.PlayerController.MovePlayer(Direction.DownRight); }
        public void MoveDown() { GameController.Instance.PlayerController.MovePlayer(Direction.Down); }
        public void MoveDownLeft() { GameController.Instance.PlayerController.MovePlayer(Direction.DownLeft); }
        public void MoveLeft() { GameController.Instance.PlayerController.MovePlayer(Direction.Left); }
        public void MoveUpLeft() { GameController.Instance.PlayerController.MovePlayer(Direction.UpLeft); }
        public void Interact() { GameController.Instance.PlayerController.PerformAction(); }

        public void ToggleStatus() { GameController.Instance.UIController.ToggleUIVisibility(); }
    }
}
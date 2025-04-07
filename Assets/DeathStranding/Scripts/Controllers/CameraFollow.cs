using UnityEngine;

namespace ALUNGAMES
{
    /// <summary>
    /// 相机跟随玩家位置的脚本，应该附加到主相机上
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("跟随设置")]
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10); // 相机偏移量，默认z为-10以便在2D模式下看到场景
        [SerializeField] private float smoothSpeed = 5f; // 平滑跟随速度
        [SerializeField] private bool usePixelPerfect = true; // 是否使用像素完美跟随
        
        [Header("边界设置")]
        [SerializeField] private bool useBoundaries = false; // 是否限制相机移动范围
        [SerializeField] private float minX = -10f;
        [SerializeField] private float maxX = 10f;
        [SerializeField] private float minY = -10f;
        [SerializeField] private float maxY = 10f;
        
        [Header("高级设置")]
        [SerializeField] private bool lookAhead = false; // 是否向前看（玩家移动方向）
        [SerializeField] private float lookAheadFactor = 2f; // 向前看的系数
        [SerializeField] private bool useGridSnapping = true; // 是否使用网格对齐
        [SerializeField] private float gridSize = 1f; // 网格大小
        
        // 缓存的引用
        private Transform target; // 目标变换
        private Vector3 velocity = Vector3.zero;
        private Vector2Int lastPlayerPosition;
        private PlayerController playerController;
        
        private void Start()
        {
            // 尝试从GameController获取PlayerController
            if (GameController.Instance != null && GameController.Instance.PlayerController != null)
            {
                playerController = GameController.Instance.PlayerController;
                
                // 初始化上一次位置
                lastPlayerPosition = playerController.PlayerPosition;
                
                // 设置初始相机位置
                UpdateCameraPosition(true);
            }
            else
            {
                Debug.LogError("CameraFollow: 无法获取PlayerController");
            }
        }
        
        private void LateUpdate()
        {
            if (playerController == null)
            {
                // 如果PlayerController为空，尝试重新获取
                if (GameController.Instance != null && GameController.Instance.PlayerController != null)
                {
                    playerController = GameController.Instance.PlayerController;
                }
                else
                {
                    return;
                }
            }
            
            // 检查玩家是否移动
            Vector2Int currentPlayerPosition = playerController.PlayerPosition;
            if (currentPlayerPosition != lastPlayerPosition)
            {
                lastPlayerPosition = currentPlayerPosition;
                UpdateCameraPosition(false);
            }
        }
        
        /// <summary>
        /// 更新相机位置
        /// </summary>
        /// <param name="immediate">是否立即更新位置（不使用平滑过渡）</param>
        private void UpdateCameraPosition(bool immediate = false)
        {
            if (playerController == null) return;
            
            // 获取玩家位置并转换为世界坐标
            Vector3 playerPosition = new Vector3(
                playerController.PlayerPosition.x,
                playerController.PlayerPosition.y,
                0
            );
            
            // 计算目标位置
            Vector3 targetPosition = playerPosition + offset;
            
            // 网格对齐
            if (useGridSnapping)
            {
                targetPosition.x = Mathf.Round(targetPosition.x / gridSize) * gridSize;
                targetPosition.y = Mathf.Round(targetPosition.y / gridSize) * gridSize;
            }
            
            // 应用边界限制
            if (useBoundaries)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
            }
            
            // 更新相机位置
            if (immediate)
            {
                transform.position = targetPosition;
            }
            else
            {
                // 使用平滑过渡
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    targetPosition,
                    ref velocity,
                    1f / smoothSpeed
                );
            }
        }
        
        /// <summary>
        /// 立即设置相机位置到玩家位置
        /// </summary>
        public void JumpToPlayer()
        {
            UpdateCameraPosition(true);
        }
        
        /// <summary>
        /// 设置新的相机偏移量
        /// </summary>
        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
            UpdateCameraPosition();
        }
        
        /// <summary>
        /// 设置边界值
        /// </summary>
        public void SetBoundaries(float newMinX, float newMaxX, float newMinY, float newMaxY)
        {
            minX = newMinX;
            maxX = newMaxX;
            minY = newMinY;
            maxY = newMaxY;
            useBoundaries = true;
            
            // 更新位置以应用新边界
            UpdateCameraPosition();
        }
    }
} 
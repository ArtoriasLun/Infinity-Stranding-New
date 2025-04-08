using System.Collections.Generic;
using UnityEngine;

namespace ALUNGAMES
{
    public class PlayerController : MonoBehaviour
    {
        // 将玩家初始位置设置在地图中心，这样更容易与城市重叠
        private Vector2Int playerPosition = new Vector2Int(20, 20);
        private int moveCount = 0; // 累计移动计数，用于地形阻力
        private TerrainType[,] terrain;
        private int carriedCargo = 0;
        private float bitcoin = 0;
        private int strain = 0;
        private List<Task> tasks = new List<Task>();

        // 玩家当前世界坐标
        private int currentWorldX = 0;
        private int currentWorldY = 0;

        public Vector2Int PlayerPosition => playerPosition;
        public int CarriedCargo => carriedCargo;
        public float Bitcoin => bitcoin;
        public int Strain => strain;
        public List<Task> Tasks => tasks;

        // 事件委托
        public delegate void PlayerMovedDelegate();
        public event PlayerMovedDelegate OnPlayerMoved;
        
        public delegate void PlayerChangedChunkDelegate(int newX, int newY);
        public event PlayerChangedChunkDelegate OnPlayerChangedChunk;

        // 初始化玩家
        public void InitializePlayer(TerrainType[,] newTerrain)
        {
            terrain = newTerrain;
            moveCount = 0;
        }

        // 移动玩家
        public void MovePlayer(Direction direction)
        {
            Vector2Int directionVector = DirectionToVector(direction);
            int dx = directionVector.x;
            int dy = directionVector.y;
            
            int newX = playerPosition.x + dx;
            int newY = playerPosition.y + dy;
            int mapHeight = terrain.GetLength(0);
            int mapWidth = terrain.GetLength(1);
            
            // 获取GameConfig
            DeathStrandingConfig gameConfig = GameController.Instance.GameConfig;
            
            // 检查是否移出地图边界
            if (newX < 0 || newX >= mapWidth || newY < 0 || newY >= mapHeight)
            {
                // 处理世界区块切换
                if (newX < 0 && currentWorldX > 0)
                {
                    currentWorldX--;
                    playerPosition.x = mapWidth - 1;
                    OnPlayerChangedChunk?.Invoke(currentWorldX, currentWorldY);
                    return;
                }
                else if (newX >= mapWidth && currentWorldX < gameConfig.worldWidth - 1)
                {
                    currentWorldX++;
                    playerPosition.x = 0;
                    OnPlayerChangedChunk?.Invoke(currentWorldX, currentWorldY);
                    return;
                }
                else if (newY < 0 && currentWorldY > 0)
                {
                    currentWorldY--;
                    playerPosition.y = mapHeight - 1;
                    OnPlayerChangedChunk?.Invoke(currentWorldX, currentWorldY);
                    return;
                }
                else if (newY >= mapHeight && currentWorldY < gameConfig.worldHeight - 1)
                {
                    currentWorldY++;
                    playerPosition.y = 0;
                    OnPlayerChangedChunk?.Invoke(currentWorldX, currentWorldY);
                    return;
                }
                
                // 如果是边界，不移动
                return;
            }
            
            // 检查目标是否可以通行
            TerrainType nextTile = terrain[newY, newX];
            if (!IsTilePassable(nextTile))
            {
                // 如果是山脉，增加疲劳
                if (nextTile == TerrainType.Mountain && carriedCargo > 0)
                {
                    if (Random.value < GetTerrainStrainChance(TerrainType.Mountain))
                    {
                        strain = Mathf.Min(strain + GetTerrainStrainAmount(TerrainType.Mountain), gameConfig.maxStrain);
                        CheckCargoDropDueToStrain();
                    }
                }
                return;
            }
                
            TerrainType currentTile = terrain[playerPosition.y, playerPosition.x];
            moveCount++;
            
            // 处理当前位置或移动到新位置
            ProcessMovement(nextTile, newX, newY);
        }
        
        // 新增：原地等待方法
        public void Wait()
        {
            // 不移动，直接处理当前位置
            TerrainType currentTile = terrain[playerPosition.y, playerPosition.x];
            ProcessMovement(currentTile, playerPosition.x, playerPosition.y, true);
        }
        
        // 提取的共享方法：处理移动或等待
        private void ProcessMovement(TerrainType targetTile, int targetX, int targetY, bool isWaiting = false)
        {
            // 获取GameConfig
            DeathStrandingConfig gameConfig = GameController.Instance.GameConfig;
            
            // 处理地形阻力（山脉和河流需要更多步数通过）
            int terrainResistance = GetTerrainMoveResistance(targetTile);
            
            if (!isWaiting && moveCount < terrainResistance)
            {
                // 还不能移动，但可能增加疲劳
                if (carriedCargo > 0)
                {
                    if (targetTile == TerrainType.Mountain && Random.value < GetTerrainStrainChance(TerrainType.Mountain))
                    {
                        strain = Mathf.Min(strain + GetTerrainStrainAmount(TerrainType.Mountain), gameConfig.maxStrain);
                    }
                    else if (targetTile == TerrainType.Water && Random.value < GetTerrainStrainChance(TerrainType.Water))
                    {
                        strain = Mathf.Min(strain + GetTerrainStrainAmount(TerrainType.Water), gameConfig.maxStrain);
                    }
                }
                return; // 如果还不能移动，直接返回
            }
            
            // 可以移动或者是原地等待
            if (!isWaiting) {
                // 只有非等待状态才更新位置
                playerPosition.x = targetX;
                playerPosition.y = targetY;
                moveCount = 0;
            }
            
            // 移动后或等待时可能增加疲劳
            if (carriedCargo > 0)
            {
                float strainChance = GetTerrainStrainChance(targetTile);
                if (strainChance > 0 && Random.value < strainChance)
                {
                    int strainAmount = GetTerrainStrainAmount(targetTile);
                    strain = Mathf.Min(strain + strainAmount, gameConfig.maxStrain);
                    
                    // 检查是否需要掉落货物
                    CheckCargoDropDueToStrain();
                }
            }
            
            // 通知移动
            OnPlayerMoved?.Invoke();
        }
        
        // 检查因疲劳导致的货物掉落
        private void CheckCargoDropDueToStrain()
        {
            DeathStrandingConfig gameConfig = GameController.Instance.GameConfig;
            
            if (strain >= gameConfig.maxStrain && carriedCargo > 0 && tasks.Count > 0 && 
                Random.value < gameConfig.strainDropChance)
            {
                int randomTaskIndex = Random.Range(0, tasks.Count);
                Task failedTask = tasks[randomTaskIndex];
                
                Debug.Log("You're too strained! The cargo was dropped!");
                
                tasks.RemoveAt(randomTaskIndex);
                carriedCargo--;
                strain = 0;
            }
        }
        
        // 执行交互动作
        public void PerformAction()
        {
            // 通过GameController.Instance获取CityManager
            CityManager cityManager = GameController.Instance.CityManager;
            
            // 处理城市内的行动
            if (cityManager.IsCityChunk(currentWorldX, currentWorldY))
            {
                Debug.Log("is city action");
                if (cityManager.HandleCityAction(playerPosition, terrain, currentWorldX, currentWorldY, 
                                               ref tasks, ref carriedCargo, ref bitcoin, ref strain))
                {
                    OnPlayerMoved?.Invoke(); // 通知UI更新
                }
            }
        }
        
        // 将方向转为向量
        private Vector2Int DirectionToVector(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return new Vector2Int(0, -1);
                case Direction.UpRight:
                    return new Vector2Int(1, -1);
                case Direction.Right:
                    return new Vector2Int(1, 0);
                case Direction.DownRight:
                    return new Vector2Int(1, 1);
                case Direction.Down:
                    return new Vector2Int(0, 1);
                case Direction.DownLeft:
                    return new Vector2Int(-1, 1);
                case Direction.Left:
                    return new Vector2Int(-1, 0);
                case Direction.UpLeft:
                    return new Vector2Int(-1, -1);
                default:
                    return Vector2Int.zero;
            }
        }
        
        // 检查地形是否可通过
        private bool IsTilePassable(TerrainType tileType)
        {
            switch (tileType)
            {
                case TerrainType.Empty:
                case TerrainType.Road:
                case TerrainType.Grass:
                case TerrainType.Mountain:
                case TerrainType.Water:
                case TerrainType.CityGate:
                case TerrainType.BuildingGate:
                    return true;
                case TerrainType.Tree:
                case TerrainType.LargeTree:
                case TerrainType.CityWall:
                case TerrainType.BuildingWall:
                case TerrainType.Bar:
                case TerrainType.Yard:
                case TerrainType.Hotel:
                case TerrainType.Exchange:
                    return false;
                default:
                    return true;
            }
        }
        
        // 获取地形移动阻力
        private int GetTerrainMoveResistance(TerrainType terrainType)
        {
            DeathStrandingConfig gameConfig = GameController.Instance.GameConfig;
            
            switch (terrainType)
            {
                case TerrainType.Mountain:
                    return gameConfig.terrain.mountain.moveResistance;
                case TerrainType.Water:
                    return gameConfig.terrain.river.moveResistance;
                case TerrainType.Empty:
                case TerrainType.Road:
                case TerrainType.Grass:
                    return 1;
                default:
                    return 1;
            }
        }
        
        // 获取地形造成疲劳的几率
        private float GetTerrainStrainChance(TerrainType terrainType)
        {
            DeathStrandingConfig gameConfig = GameController.Instance.GameConfig;
            
            switch (terrainType)
            {
                case TerrainType.Mountain:
                    return gameConfig.terrain.mountain.strainChance;
                case TerrainType.Water:
                    return gameConfig.terrain.river.strainChance;
                case TerrainType.Empty:
                case TerrainType.Road:
                case TerrainType.Grass:
                    return 0f;
                default:
                    return 0f;
            }
        }
        
        // 获取地形造成的疲劳值
        private int GetTerrainStrainAmount(TerrainType terrainType)
        {
            DeathStrandingConfig gameConfig = GameController.Instance.GameConfig;
            
            switch (terrainType)
            {
                case TerrainType.Mountain:
                    return gameConfig.terrain.mountain.strainAmount;
                case TerrainType.Water:
                    return gameConfig.terrain.river.strainAmount;
                case TerrainType.Empty:
                case TerrainType.Road:
                case TerrainType.Grass:
                    return 0;
                default:
                    return 0;
            }
        }
        
        // 设置当前地形
        public void SetTerrain(TerrainType[,] newTerrain)
        {
            terrain = newTerrain;
        }
        
        // 获取当前世界X坐标
        public int GetCurrentWorldX()
        {
            return currentWorldX;
        }
        
        // 获取当前世界Y坐标
        public int GetCurrentWorldY()
        {
            return currentWorldY;
        }
        
        // 添加任务
        public void AddTask(Task task)
        {
            tasks.Add(task);
            carriedCargo++;
        }
        
        // 完成任务
        public void CompleteTask(Task task)
        {
            if (tasks.Contains(task))
            {
                bitcoin += task.Reward;
                tasks.Remove(task);
                carriedCargo--;
            }
        }
        
        // 设置当前坐标
        public void SetCurrentWorldPosition(int x, int y)
        {
            currentWorldX = x;
            currentWorldY = y;
        }
        
        // 强制触发地图渲染
        public void ForceMapRender()
        {
            OnPlayerChangedChunk?.Invoke(currentWorldX, currentWorldY);
        }
    }
} 
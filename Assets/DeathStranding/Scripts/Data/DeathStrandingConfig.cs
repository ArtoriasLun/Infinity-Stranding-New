using System;
using UnityEngine;

namespace ALUNGAMES
{
    [CreateAssetMenu(fileName = "DeathStrandingConfig", menuName = "😊/Game Config")]
    public class DeathStrandingConfig : ScriptableObject
    {
        [Header("Player Attributes")]
        public int maxCargo = 3;          // 最大携带货物数量
        public int maxStrain = 100;       // 最大疲劳值
        public int moveSpeed = 1;         // 基础移动速度
        
        [Header("Terrain Effects")]
        public TerrainEffects terrain;
        
        [Header("World Generation Parameters")]
        [Header("地图尺寸")]
        public int mapWidth = 40;
        public int mapHeight = 30;
        public int worldWidth = 10;
        public int worldHeight = 10;
        
        [Header("地形生成")]
        public float mountainThreshold = 0.6f;     // 高度图中超过此值生成山脉
        public float grassThreshold = 0.4f;        // 高度图中超过此值可能生成草地
        public float grassChance = 0.3f;           // 草地生成概率
        
        [Header("City Generation")]
        [Range(2, 10)]
        public int minCityCount = 2;                 // 最小城市数量
        [Range(2, 10)]
        public int maxCityCount = 5;                 // 最大城市数量
        public int cityCount = 5;                  // 世界地图中的城市数量
        public int citySize = 20;                  // 城市大小
        
        [Header("River Generation")]
        public int riverCount = 2;                 // 每个区块的河流数
        public int riverMinLength = 5;             // 河流最小长度
        public float riverBranchChance = 0.1f;     // 河流分支概率
        
        [Header("Tree Generation")]
        public int treeMaxCount = 20;              // 每个区块的最大树木数量
        public int smallTreeWeight = 1;            // 小树的权重
        public int largeTreeWeight = 4;            // 大树的权重
        public float largeTreeChance = 0.3f;       // 生成大树的概率
        
        [Header("Task System")]
        public TaskSettings tasks;
        
        [Header("Game Mechanics")]
        public float strainDropChance = 1.0f;      // 当strain达到最大时掉落货物的几率
        
        [Serializable]
        public class TerrainEffects
        {
            [Header("River")]
            public RiverEffects river;
            
            [Header("Mountain")]
            public MountainEffects mountain;
            
            [Serializable]
            public class RiverEffects
            {
                public float strainChance = 1.0f;
                public int strainAmount = 10;
                public int moveResistance = 2;
            }
            
            [Serializable]
            public class MountainEffects
            {
                public float strainChance = 0.3f;
                public int strainAmount = 10;
                public int moveResistance = 3;
            }
        }
        
        [Serializable]
        public class TaskSettings
        {
            public float bitcoinReward = 0.33f;        // 每个货物的比特币奖励
        }
        
        // 初始化默认值
        private void OnEnable()
        {
            // 确保所有引用类型都已初始化
            if (terrain == null)
                terrain = new TerrainEffects();
                
            if (terrain.river == null)
                terrain.river = new TerrainEffects.RiverEffects();
                
            if (terrain.mountain == null)
                terrain.mountain = new TerrainEffects.MountainEffects();
                
            if (tasks == null)
                tasks = new TaskSettings();
        }
    }
} 
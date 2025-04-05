// 游戏数值配置管理中心
// 所有游戏数值参数集中在此处，方便调整和平衡

const GameConfig = {
    // 玩家属性
    player: {
        maxCargo: 3,          // 最大携带货物数量
        maxStrain: 100,       // 最大疲劳值
        moveSpeed: 1,         // 基础移动速度
    },
    
    // 地形影响
    terrain: {
        // 河流
        river: {
            strainChance: 1,      // 河流增加疲劳的几率
            strainAmount: 10,       // 河流增加的疲劳值
            moveResistance: 2,      // 河流的移动阻力（移动需要的步数）
        },
        
        // 山脉
        mountain: {
            strainChance: 0.3,      // 山脉增加疲劳的几率
            strainAmount: 10,       // 山脉增加的疲劳值
            moveResistance: 3,      // 山脉的移动阻力
        }
    },
    
    // 世界生成参数
    worldGen: {
        // 地图尺寸
        mapSize: 40,
        worldWidth: 10,
        worldHeight: 10,
        
        // 地形生成
        mountainThreshold: 0.6,     // 高度图中超过此值生成山脉
        grassThreshold: 0.4,        // 高度图中超过此值可能生成草地
        grassChance: 0.3,           // 草地生成概率
        
        // 城市生成
        cityCount: 5,               // 世界地图中的城市数量
        citySize: 20,               // 城市大小
        
        // 河流生成
        riverCount: 2,              // 每个区块的河流数
        riverMinLength: 5,          // 河流最小长度
        riverBranchChance: 0.1,     // 河流分支概率
        
        // 树木生成
        treeMaxWeight: 6,           // 每个区块的最大树木权重
        smallTreeWeight: 1,         // 小树权重
        largeTreeWeight: 4,         // 大树权重
        largeTreeChance: 0.3,       // 生成大树的概率
    },
    
    // 任务系统
    tasks: {
        bitcoinReward: 0.33,        // 每个货物的比特币奖励
    },
    
    // 游戏机制
    mechanics: {
        strainDropChance: 1.0,      // 当strain达到最大时掉落货物的几率
    }
};

// 不要修改以下导出语句
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { GameConfig };
} 
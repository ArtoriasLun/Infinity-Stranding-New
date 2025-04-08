# DeathStrandingConfig 技术文档

## 配置文件概述
`DeathStrandingConfig` 是一个ScriptableObject配置类，用于集中管理游戏的核心参数设置。

## 主要功能
- 控制世界生成规则
- 定义玩家基础属性
- 配置地形效果参数
- 管理任务系统设置

## 参数详解

### 世界设置
| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| worldSeed | float | 100f | 世界生成种子，影响地形、河流等随机生成 |
| mapWidth | int | 40 | 单个区块的宽度 |
| mapHeight | int | 30 | 单个区块的高度 |
| worldWidth | int | 10 | 世界横向区块数量 |
| worldHeight | int | 10 | 世界纵向区块数量 |

### 地形生成
| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| mountainThreshold | float | 0.6f | 山地生成高度阈值 |
| grassThreshold | float | 0.2f | 草地生成高度阈值 |
| grassChance | float | 0.5f | 符合条件区域生成草地的概率 |

### 城市生成
| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| cityCount | int | 5 | 世界中的城市数量 |
| citySize | int | 20 | 城市占据的区块大小 |

### 河流生成
| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| riverCount | int | 2 | 每个区块的河流数量 |
| riverMinLength | int | 5 | 河流最小长度 |
| riverBranchChance | float | 0.1f | 河流分支概率 |

### 树木生成
| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| treeMaxCount | int | 20 | 每个区块最大树木数量 |
| smallTreeWeight | int | 1 | 小树权重 |
| largeTreeWeight | int | 4 | 大树权重 |
| largeTreeChance | float | 0.3f | 生成大树的概率 |

// 应用配置
GameController.Instance.GameConfig = config;

## 调试建议
1. 修改worldSeed可以生成不同的随机地图
2. 降低grassThreshold会增加草地生成区域
3. 提高riverCount可以创建更多河流
4. 调整treeMaxCount控制树木密度

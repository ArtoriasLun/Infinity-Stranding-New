# TerrainGenerator 文档

## 概述
`TerrainGenerator` 是一个 Unity MonoBehaviour，负责程序化生成游戏世界的地形。它可以创建不同类型的地形包括山脉、草地、水域（河流）、树木和城市布局。

## 类成员

### `GenerateTerrain(int mapWidth, int mapHeight, int currentWorldX, int currentWorldY)`
生成一个表示游戏世界地形的 TerrainType 二维数组。

**参数:**
- `mapWidth`: 地形图的宽度（必须为正数）
- `mapHeight`: 地形图的高度（必须为正数）
- `currentWorldX`: 世界区块的X坐标
- `currentWorldY`: 世界区块的Y坐标

**返回值:**
表示生成地形的 TerrainType 二维数组

**行为:**
1. 验证输入参数，如果无效则默认为40x40
2. 初始化地形数组
3. 从GameController获取配置
4. 使用柏林噪声生成高度图
5. 根据高度值填充地形：
   - 高于mountainThreshold的生成山脉
   - 在grassThreshold和mountainThreshold之间的生成草地
   - 其他情况生成道路
6. 根据配置生成河流
7. 随机放置树木
8. 城市区块由CityManager特殊处理

### `GenerateHeightMap(float seed)`
使用噪声函数生成高度图。

**参数:**
- `seed`: 噪声生成的随机种子

**返回值:**
表示地形高度的浮点数二维数组

### `GenerateMountainsAndGrass(float[,] heightMap)`
根据高度图生成山脉和草地。

**参数:**
- `heightMap`: 预生成的高度图

### `CarveRiver(int startX, int startY, float[,] heightMap)`
在指定坐标创建河流。

**参数:**
- `startX`: 河流起始X坐标
- `startY`: 河流起始Y坐标
- `heightMap`: 参考高度图

### `BranchRiver(int x, int y, int parentDx, int parentDy)`
从主河流创建分支。

**参数:**
- `x`: 分支起始X坐标
- `y`: 分支起始Y坐标
- `parentDx`: 主河流X方向
- `parentDy`: 主河流Y方向

### `GenerateTrees()`
在有效地形上随机放置树木。

### 噪声函数
- `SmoothNoise`: 从基础噪声创建平滑噪声
- `Noise`: 使用正弦波的基础噪声函数

## 依赖项
- 需要 `GameController.Instance` 获取配置
- 使用 `CityManager` 生成城市地形
- 依赖 `DeathStrandingConfig` 获取生成参数

## 错误处理
- 捕获并记录异常
- 出错时回退到简单道路地形
- 验证输入参数并提供默认值

## 配置
由 `DeathStrandingConfig` 控制:
- `mountainThreshold`: 山脉高度阈值
- `grassThreshold`: 草地高度阈值
- `riverCount`: 要生成的河流数量
- `riverMinLength`: 河流最小长度
- `riverBranchChance`: 河流分支概率
- `treeMaxCount`: 最大树木权重
- `smallTreeWeight`: 小树权重
- `largeTreeWeight`: 大树权重
- `largeTreeChance`: 大树生成概率

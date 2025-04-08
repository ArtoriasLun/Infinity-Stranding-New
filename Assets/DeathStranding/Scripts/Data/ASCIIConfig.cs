using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

[CreateAssetMenu(fileName = "ASCIIConfig", menuName = "😊/ASCIIConfig")]
public class ASCIIConfig : ScriptableObject
{
    [System.Serializable]
    public struct TileConfig
    {
        public int id;              // Tile的唯一ID
        public string name;         // 名称
        public char asciiChar;      // ASCII字符
        public Color color;         // 颜色
        public Sprite sprite;       // 精灵图
        public bool isPassable;     // 是否可通过
        public int moveResistance;  // 移动阻力
        public float strainChance;  // 疲劳概率
        public int strainAmount;    // 疲劳量
        public bool isWall;         // 是否是墙
        public bool isGate;         // 是否是门
        public bool isSpecialPoint; // 是否是特殊点
    }

    public TileConfig[] tiles;  // 所有tile的配置

    // 游戏参数配置
    public int maxCargo = 100;         // 最大货物量
    public int maxStrain = 100;        // 最大疲劳值
    public float bitcoinReward = 10f;  // 任务奖励
    public float mountainStrainChance = 0.3f;  // 山地增加疲劳概率
    public int mountainStrainAmount = 10;      // 山地疲劳增加量
    public int waterStrainAmount = 10;         // 河流疲劳增加量
    public int mountainMoveResistance = 3;     // 山地移动阻力
    public int waterMoveResistance = 2;        // 河流移动阻力

    // 快速查找表
    private Dictionary<int, TileConfig> tileConfigsById;
    private Dictionary<char, List<TileConfig>> tileConfigsByChar;
    private void OnEnable()
    {
        InitializeLookupTables();
    }

    private void InitializeLookupTables()
    {
        // 初始化ID查找表
        tileConfigsById = new Dictionary<int, TileConfig>();
        tileConfigsByChar = new Dictionary<char, List<TileConfig>>();

        foreach (var tile in tiles)
        {
            // ID查找表
            tileConfigsById[tile.id] = tile;

            // 字符查找表
            if (!tileConfigsByChar.ContainsKey(tile.asciiChar))
            {
                tileConfigsByChar[tile.asciiChar] = new List<TileConfig>();
            }
            tileConfigsByChar[tile.asciiChar].Add(tile);
        }
    }

    // 根据ID获取配置
    public TileConfig GetTileConfig(int id)
    {
        return tileConfigsById.TryGetValue(id, out var config) ? config : default;
    }

    // 根据字符获取所有可能的配置
    public List<TileConfig> GetTileConfigsByChar(char c, string key)
    {
        // 获取原始结果列表
        List<TileConfig> originalResult = tileConfigsByChar.TryGetValue(c, out var configs)
            ? configs : new List<TileConfig>();

        if (!string.IsNullOrEmpty(key))
        {
            // 创建过滤后的副本
            var filtered = originalResult
                .Where(t => t.name.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            // 回退机制：仅当过滤后有结果时返回
            return filtered.Count > 0 ? filtered : originalResult;
        }

        return originalResult;
    }

    // 将所有颜色设置为白色
    public void SetAllColorsToWhite()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            var config = tiles[i];
            config.color = Color.white;
            tiles[i] = config;
        }

        // 重新初始化查找表
        InitializeLookupTables();
    }
}
